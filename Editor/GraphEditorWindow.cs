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
        private string _graphPropertyPath;
        [SerializeField, HideInInspector]
        private int _graphOwnerInstanceId;

        public IGraph Graph { get; private set; }
        
        public GraphDrawer.GraphDrawer GraphDrawer { get; private set; }
        public SerializedProperty GraphProperty { get; private set; }
        public GraphViewSettings ViewSettings { get; private set; }

        private bool _loadGraphPropertySerializationRoot = false;
        
        public static void OpenGraphWindowForProperty<T>(Object serializationRoot, string graphPropertyPath)
            where T : GraphEditorWindow
        {
            if (TryGetOpenedGraphEditorWindowWithContent<T>(serializationRoot, graphPropertyPath, out var openedWindow))
            {
                openedWindow.Focus();
                return;
            }

            var window = OpenNewGraphEditorWindow<T>();
            
            window._graphPropertyPath = graphPropertyPath;
            window._graphPropertySerializationRoot = serializationRoot;
            window._graphOwnerInstanceId = serializationRoot.GetInstanceID();
            
            window.LoadGraphProperty();
            window.InitializeGraphDrawer();
            window.Focus();
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

            editorWindow = default;
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
        
        public virtual void OnBeforeSerialize()
        {
        }

        public virtual void OnAfterDeserialize()
        {
            _loadGraphPropertySerializationRoot = true;
        }
        
        protected virtual void OnEnable()
        {
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
            ObjectChangeEvents.changesPublished += ChangesPublished;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            if (!_loadGraphPropertySerializationRoot)
            {
                return;
            }
            _loadGraphPropertySerializationRoot = false;
            
            LoadGraphProperty();
            if (Graph != null)
            {
                InitializeGraphDrawer();
            }
        }

        protected virtual void OnDisable()
        {
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            ClearGraphProperty();
            ClearGraphDrawer();
        }

        private void LoadGraphProperty()
        {
            if (_graphPropertySerializationRoot == null)
            {
                _graphPropertySerializationRoot = EditorUtility.InstanceIDToObject(_graphOwnerInstanceId);
            }

            GraphProperty = default;
            Graph = default;
            
            if (_graphPropertySerializationRoot == null || string.IsNullOrWhiteSpace(_graphPropertyPath))
            {
                return;
            }

            var serializedObject = new SerializedObject(_graphPropertySerializationRoot);
            GraphProperty = serializedObject.FindProperty(_graphPropertyPath);
            
            if (GraphProperty == null)
            {
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
                return;
            }

            OnGraphDrawerCreated();
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
        
        private void OnSceneUnloaded(Scene scene)
        {
            ClearIfSerializationRootIsLost();
            TryReload();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            ClearIfSerializationRootIsLost();
            TryReload();
        }
        
        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            ClearIfSerializationRootIsLost();
            TryReload();
        }

        private void OnPrefabStageClosing(PrefabStage obj)
        {
            if (_graphPropertySerializationRoot is not Component component || !obj.IsPartOfPrefabContents(component.gameObject))
                return;
            
            ClearGraphDrawer();
            Close();
        }
        
        private void ClearIfSerializationRootIsLost()
        {
            if (_graphPropertySerializationRoot != null)
                return;
            
            ClearGraphProperty();
            ClearGraphDrawer();
        }

        private void TryReload()
        {
            if (GraphDrawer != null) 
                return;

            LoadGraphProperty();
            if (Graph != null)
                InitializeGraphDrawer();
        }
    }
}