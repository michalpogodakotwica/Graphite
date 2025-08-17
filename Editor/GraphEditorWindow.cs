using System;
using System.Collections.Generic;
using System.Reflection;
using Graphite.Attributes;
using Graphite.Editor.Attributes;
using Graphite.Editor.ElementDrawerProvider;
using Graphite.Editor.Settings;
using Graphite.Editor.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Graphite.Editor
{
    /// Parent for graph view. Serializes property path to graph and reopens editor on Unity restart.
    public class GraphEditorWindow : EditorWindow, ISerializationCallbackReceiver
    {
        private static readonly DrawerTypeMapping<IGraph, GraphDrawer.GraphDrawer, CustomGraphDrawerAttribute>
            GraphDrawerMapping = new();

        [SerializeField, HideInInspector]
        private Object _graphPropertySerializationRoot;
        [SerializeField, HideInInspector]
        private string _graphSerializationRootGlobalId;
        [SerializeField, HideInInspector]
        private string _graphPropertyPath;
        [SerializeField, HideInInspector]
        private bool _loadGraphPropertySerializationRoot;
        
        public IGraph Graph { get; private set; }
        public GraphDrawer.GraphDrawer GraphDrawer { get; private set; }
        public SerializedProperty GraphProperty { get; private set; }
        public GraphViewSettings ViewSettings { get; private set; }

        private bool _wasInitialized;
        
        public static void OpenGraphWindowForProperty<T>(Object serializationRoot, string graphPropertyPath)
            where T : GraphEditorWindow
        {
            if (TryGetOpenedGraphEditorWindowWithContent<T>(serializationRoot, graphPropertyPath, out var openedWindow))
            {
                openedWindow.Focus();
                return;
            }

            var window = OpenNewGraphEditorWindow<T>();
            
            window._graphPropertySerializationRoot = serializationRoot;
            window._graphPropertyPath = graphPropertyPath;
            window._graphSerializationRootGlobalId = GlobalObjectId.GetGlobalObjectIdSlow(serializationRoot).ToString();
            
            window.LoadGraphProperty();
            window.InitializeGraphDrawer();
        }
        
        protected static bool TryGetOpenedGraphEditorWindowWithContent<T>(Object serializationRoot, string propertyPath,
            out T editorWindow) where T : GraphEditorWindow
        {
            var openedEditors = GetOpenedGraphEditorWindows<T>();

            foreach (var openedEditor in openedEditors)
            {
                if (openedEditor._graphPropertySerializationRoot != serializationRoot ||
                    openedEditor._graphPropertyPath != propertyPath)
                {
                    continue;
                }

                editorWindow = openedEditor;
                return true;
            }

            editorWindow = null;
            return false;
        }

        protected static T OpenNewGraphEditorWindow<T>() where T : GraphEditorWindow
        {
            var sceneViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneView");
            return CreateWindow<T>(typeof(T), sceneViewType);
        }

        protected static IEnumerable<T> GetOpenedGraphEditorWindows<T>() where T : GraphEditorWindow
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }
        
        protected virtual void OnEnable()
        {
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            ObjectChangeEvents.changesPublished += ChangesPublished;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            EditorApplication.playmodeStateChanged += OnModeChanged;
            
            if (!_loadGraphPropertySerializationRoot)
                return;
            
            _loadGraphPropertySerializationRoot = false;
            LoadGraphProperty();
            if (Graph != null)
                InitializeGraphDrawer();
        }

        private void OnInspectorUpdate()
        {
            if (_graphPropertySerializationRoot == null)
                Close();
        }
        
        protected virtual void OnDisable()
        {
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            EditorApplication.playmodeStateChanged -= OnModeChanged;

            ClearGraphProperty();
            ClearGraphDrawer();
        }

        protected void OnFocus()
        {
            if (_wasInitialized && _graphPropertySerializationRoot == null)
            {
                Close();
            }
        }

        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            _loadGraphPropertySerializationRoot = true;
        }
        
        private void LoadGraphProperty()
        {
            GraphProperty = null;
            Graph = null;
            
            if (_graphPropertySerializationRoot == null)
            {
                if (!GlobalObjectId.TryParse(_graphSerializationRootGlobalId, out var globalObjectId))
                {
                    Close();
                    return;
                }
                _graphPropertySerializationRoot = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                if (_graphPropertySerializationRoot == null)
                {
                    Close();
                    return;
                }
            }
            
            var serializedObject = new SerializedObject(_graphPropertySerializationRoot);
            GraphProperty = serializedObject.FindProperty(_graphPropertyPath);
            if (GraphProperty == null)
            {
                Close();
                return;
            }
            
            InitializeSettings();
            Graph = (IGraph)GraphProperty?.GetValue();
        }
        
        private void InitializeSettings()
        {
            if (ViewSettings != null) 
                return;
            
            ViewSettings = GetGraphEditorSettings();
            if (ViewSettings == null)
            {
                return;
            }
            OnGraphSettingsLoaded();
        }

        protected GraphViewSettings GetGraphEditorSettings()
        {
            var graphDrawingSettingsAttribute =
                GraphProperty.GetFieldInfo().GetCustomAttribute<GraphViewSettingsAttribute>();
            
            return graphDrawingSettingsAttribute != null
                ? new GraphViewSettings(GraphProperty, graphDrawingSettingsAttribute)
                : new GraphViewSettings(GraphProperty, Graph);
        }        
        
        protected void OnGraphSettingsLoaded()
        {
            titleContent = new GUIContent(ViewSettings.DisplaySettings.Title);
        }
        
        private void InitializeGraphDrawer()
        {
            Assert.IsNull(GraphDrawer);
            Assert.IsNotNull(Graph);
            
            var graphDrawerType = GraphDrawerMapping.GetDrawerForType(Graph.GetType());
            GraphDrawer = (GraphDrawer.GraphDrawer)Activator.CreateInstance(
                graphDrawerType,
                new object[] { this }
            );
            if (GraphDrawer == null)
            {
                Close();
                return;
            }
            OnGraphDrawerCreated();
            _wasInitialized = true;
        }
        
        protected virtual void OnGraphDrawerCreated()
        {
            rootVisualElement.Add(GraphDrawer);
            GraphDrawer.StretchToParentSize();
            GraphDrawer.RedrawGraph();
        }

        private void ClearGraphProperty()
        {
            GraphProperty = null;
            Graph = null;
        }
        
        private void ClearGraphDrawer()
        {
            if (GraphDrawer == null)
                return;
            
            GraphDrawer.Dispose();
            GraphDrawer.RemoveFromHierarchy();
            GraphDrawer = null;
            
            OnGraphDrawerCleared();
        }

        protected virtual void OnGraphDrawerCleared()
        {
        }
        
        private void OnModeChanged()
        {
            if (_graphPropertySerializationRoot == null)
                Close();
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            if (_graphPropertySerializationRoot == null)
                Close();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_graphPropertySerializationRoot == null)
                Close();
        }
        
        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (_graphPropertySerializationRoot == null)
                Close();
        }

        private void OnPrefabStageClosing(PrefabStage obj)
        {
            if (_graphPropertySerializationRoot is Component component &&
                obj.IsPartOfPrefabContents(component.gameObject))
                Close();
        }
    }
}