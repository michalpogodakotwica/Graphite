using System;
using System.Collections.Generic;
using System.Reflection;
using Attributes;
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;
using com.michalpogodakotwica.graphite.Editor.Settings;
using com.michalpogodakotwica.graphite.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace com.michalpogodakotwica.graphite.Editor
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
        private string _sceneGraphOwnerComponentPath;
        [SerializeField, HideInInspector]
        private int _sceneGraphOwnerComponentIndex;
        [SerializeField, HideInInspector]
        private string _sceneGraphOwnerComponentType;

        public IGraph Graph { get; private set; }
        
        public GraphDrawer.GraphDrawer GraphDrawer { get; private set; }
        public SerializedProperty GraphProperty { get; private set; }
        public GraphViewSettings ViewSettings { get; private set; }

        private bool _isLoading = false;

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
            window.OnBeforeSerialize();
            window.InitializeGraphDrawer();
            window.Focus();
        }

        protected virtual void OnEnable()
        {
            ObjectChangeEvents.changesPublished += ChangesPublished;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            if (!_isLoading)
            {
                return;
            }

            _isLoading = false;
            
            if (GraphDrawer == null || (_graphPropertySerializationRoot == null && _sceneGraphOwnerComponentPath != null))
                InitializeGraphDrawer();
            
            if (_graphPropertySerializationRoot == null)
                Close();
        }

        private void OnInspectorUpdate()
        {
            if (GraphDrawer == null || (_graphPropertySerializationRoot == null && _sceneGraphOwnerComponentPath != null))
                InitializeGraphDrawer();
            
            if (_graphPropertySerializationRoot == null)
                Close();
        }

        protected virtual void OnFocus()
        {
            GraphDrawer?.Focus();
            GraphDrawer?.SetEnabled(true);
        }

        public virtual void OnBeforeSerialize()
        {
            if (_graphPropertySerializationRoot == null)
            {
                Close();
                return;
            }

            var isSerializationRootPrefab =
                PrefabUtility.GetCorrespondingObjectFromSource(_graphPropertySerializationRoot);
            if (_graphPropertySerializationRoot is Component component && !isSerializationRootPrefab)
                SaveSceneSerializationRootData(component);
            else
                ResetSceneSerializationRootData();
        }

        public virtual void OnAfterDeserialize()
        {
            _isLoading = true;
        }

        protected virtual void OnDisable()
        {
            ObjectChangeEvents.changesPublished -= ChangesPublished;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            ClearGraphDrawer();
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
            Type sceneViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneView");
            return CreateWindow<T>(typeof(T), sceneViewType);
        }

        protected static IEnumerable<T> GetOpenedGraphEditorWindows<T>() where T : GraphEditorWindow
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }

        private void InitializeGraphDrawer()
        {
            if (_graphPropertySerializationRoot == null)
            {
                ClearGraphDrawer();
            }
            
            if (GraphProperty == null || _graphPropertySerializationRoot == null && _sceneGraphOwnerComponentPath != null)
            {
                LoadGraphProperty();
                
                if (GraphProperty == null)
                    return;

                OnGraphPropertyLoaded();
            }
            
            if (GraphProperty == null || _graphPropertySerializationRoot == null)
                return;
            
            if (Graph == null)
            {
                LoadGraphFromProperty();
                
                if (Graph == null)
                    return;

                OnGraphLoadedFromProperty();
            }

            if (ViewSettings == null)
            {
                LoadGraphSettings();
                
                if (ViewSettings == null)
                    return;
                
                OnGraphSettingsLoaded();
            }
            
            if (GraphDrawer == null)
            {
                CreateGraphDrawerFromGraph();
                
                if (GraphDrawer == null)
                    return;

                OnGraphDrawerCreated();
            }
            
            RedrawGraph();
        }

        private void LoadGraphProperty()
        {
            GraphProperty = null;
            
            if (_graphPropertySerializationRoot == null && _sceneGraphOwnerComponentPath != null)
                LoadSceneSerializationRoot();
            
            if (!TryGetSerializedProperty(_graphPropertySerializationRoot, _graphPropertyPath, out var graphProperty))
                return;
            
            GraphProperty = graphProperty;
        }
        
        protected virtual void OnGraphPropertyLoaded()
        {
        }

        protected void LoadSceneSerializationRoot()
        {
            try
            {
                var gameObject = GameObject.Find(_sceneGraphOwnerComponentPath);
                var type = Type.GetType(_sceneGraphOwnerComponentType);
                _graphPropertySerializationRoot = gameObject.GetComponents(type)[_sceneGraphOwnerComponentIndex];
            }
            catch
            {
                // ignored
            }
        }

        protected bool TryGetSerializedProperty(Object serializationRoot, string graphPropertyPath,
            out SerializedProperty serializedProperty)
        {
            if (serializationRoot == null || string.IsNullOrWhiteSpace(graphPropertyPath))
            {
                serializedProperty = default;
                return false;
            }

            var serializedObject = new SerializedObject(_graphPropertySerializationRoot);
            serializedProperty = serializedObject.FindProperty(_graphPropertyPath);
            return serializedProperty != null;
        }
        
        private void LoadGraphFromProperty()
        {
            var graph = (IGraph)GraphProperty?.GetValue();
            Graph = graph;
        }

        protected virtual void OnGraphLoadedFromProperty()
        {
        }
        
        private void LoadGraphSettings()
        {
            ViewSettings = GetGraphEditorSettings();
        }

        protected virtual GraphViewSettings GetGraphEditorSettings()
        {
            var graphDrawingSettingsAttribute =
                GraphProperty.GetFieldInfo().GetCustomAttribute<GraphViewSettingsAttribute>();
            
            return graphDrawingSettingsAttribute != null
                ? new GraphViewSettings(GraphProperty, graphDrawingSettingsAttribute)
                : new GraphViewSettings(GraphProperty, Graph);
        }        
        
        protected virtual void OnGraphSettingsLoaded()
        {
            titleContent = new GUIContent(ViewSettings.DisplaySettings.Title);
        }
        
        private void CreateGraphDrawerFromGraph()
        {
            GraphDrawer = CreateGraphDrawer(Graph);
        }
        
        protected virtual void OnGraphDrawerCreated()
        {
            rootVisualElement.Add(GraphDrawer);
            GraphDrawer.StretchToParentSize();
        }
        
        protected virtual GraphDrawer.GraphDrawer CreateGraphDrawer(IGraph graph)
        {
            var graphDrawerType = GraphDrawerMapping.GetDrawerForType(graph.GetType());
            var graphDrawer = (GraphDrawer.GraphDrawer)Activator.CreateInstance(
                graphDrawerType,
                new object[] { this }
            );
            
            return graphDrawer;
        }
        
        protected virtual void RedrawGraph()
        {
            GraphDrawer.RedrawGraph();
        }
        
        protected void ResetSceneSerializationRootData()
        {
            _sceneGraphOwnerComponentPath = null;
            _sceneGraphOwnerComponentType = null;
            _sceneGraphOwnerComponentIndex = -1;
        }
        
        protected void SaveSceneSerializationRootData(Component component)
        {
            try
            {
                _sceneGraphOwnerComponentPath = GetTransformPath(component.transform);
                _sceneGraphOwnerComponentType = component.GetType().AssemblyQualifiedName;

                var components = component.transform.GetComponents(_graphPropertySerializationRoot.GetType());
                _sceneGraphOwnerComponentIndex = Array.IndexOf(components, component);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected static string GetTransformPath(Transform current) 
        {
            if (current.parent == null)
                return "/" + current.name;
            
            return GetTransformPath(current.parent) + "/" + current.name;
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
            if (_graphPropertySerializationRoot == null)
            {
                ClearGraphDrawer();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (_graphPropertySerializationRoot == null)
            {
                ClearGraphDrawer();
            }
        }
        
        private void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (_graphPropertySerializationRoot == null)
            {
                ClearGraphDrawer();
            }
        }
    }
}