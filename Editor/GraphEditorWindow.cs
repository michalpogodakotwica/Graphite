using System;
using System.Collections.Generic;
using System.Reflection;
using com.michalpogodakotwica.graphite.Attributes;
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;
using com.michalpogodakotwica.graphite.Editor.Settings;
using com.michalpogodakotwica.graphite.Editor.Utils;
using UnityEditor;
using UnityEngine;
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

        private void OnEnable()
        {
            if (GraphDrawer != null)
                return;
            
            if (_graphPropertySerializationRoot == null && _sceneGraphOwnerComponentPath != null)
                LoadSceneSerializationRoot();
            
            LoadGraph();
        }

        private void OnInspectorUpdate()
        {
            if (_graphPropertySerializationRoot == null)
                Close();
        }

        private void OnFocus()
        {
            GraphDrawer?.Focus();
            GraphDrawer?.SetEnabled(true);
        }

        public void OnBeforeSerialize()
        {
            var isSerializationRootPrefab =
                PrefabUtility.GetCorrespondingObjectFromSource(_graphPropertySerializationRoot);

            if (_graphPropertySerializationRoot is Component component && !isSerializationRootPrefab)
                SaveSceneSerializationRootData(component);
            else
                ResetSceneSerializationRootData();
        }

        public void OnAfterDeserialize()
        {
        }
        
        private void OnDisable()
        {
            GraphDrawer?.Dispose();
            GraphDrawer?.RemoveFromHierarchy();
            GraphDrawer = null;
        }
        
        private void LoadSceneSerializationRoot()
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

        private void SaveSceneSerializationRootData(Component component)
        {
            try
            {
                _sceneGraphOwnerComponentPath = GetPath(component.transform);
                _sceneGraphOwnerComponentType = component.GetType().AssemblyQualifiedName;

                var components = component.transform.GetComponents(_graphPropertySerializationRoot.GetType());
                _sceneGraphOwnerComponentIndex = Array.IndexOf(components, component);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ResetSceneSerializationRootData()
        {
            _sceneGraphOwnerComponentPath = null;
            _sceneGraphOwnerComponentType = null;
            _sceneGraphOwnerComponentIndex = -1;
        }
        
        private static string GetPath(Transform current) 
        {
            if (current.parent == null)
                return "/" + current.name;
            
            return GetPath(current.parent) + "/" + current.name;
        }
        
        private void LoadGraph(SerializedProperty graphProperty = null)
        {
            if (graphProperty == null)
            {
                if (!TryGetSerializedProperty(_graphPropertySerializationRoot, _graphPropertyPath, out graphProperty)) 
                    return;
            }

            GraphProperty = graphProperty;
            
            var graph = (IGraph)GraphProperty?.GetValue();
            if (graph == null)
                return;
            
            Graph = graph;
            if (GraphDrawer != null)
                rootVisualElement.Remove(GraphDrawer);

            ViewSettings = GetGraphEditorSettings();
            GraphDrawer = CreateGraphView(graph);
        }

        protected virtual GraphDrawer.GraphDrawer CreateGraphView(IGraph graph)
        {
            var graphDrawerType = GraphDrawerMapping.GetDrawerForType(graph.GetType());

            GraphDrawer = (GraphDrawer.GraphDrawer)Activator.CreateInstance(
                graphDrawerType,
                new object[] { this }
            );
            
            titleContent = new GUIContent(ViewSettings.DisplaySettings.Title);
            rootVisualElement.Add(GraphDrawer);
            GraphDrawer.StretchToParentSize();
            GraphDrawer.RedrawGraph();
            
            return GraphDrawer;
        }

        protected virtual GraphViewSettings GetGraphEditorSettings()
        {
            var graphDrawingSettingsAttribute =
                GraphProperty.GetFieldInfo().GetCustomAttribute<GraphViewSettingsAttribute>();
            
            return graphDrawingSettingsAttribute != null
                ? new GraphViewSettings(graphDrawingSettingsAttribute)
                : new GraphViewSettings();
        }
        
        public static void OpenGraphViewForProperty<T>(Object serializationRoot, string graphPropertyPath) where T : GraphEditorWindow
        {
            if (TryGetOpenedGraphEditorWindowWithContent<T>(serializationRoot, graphPropertyPath, out var openedWindow))
            {
                openedWindow.Focus();
                return;
            }
            
            var window = OpenNewGraphEditorWindow<T>();
            
            window._graphPropertyPath = graphPropertyPath;
            window._graphPropertySerializationRoot = serializationRoot;

            window.GraphDrawer = null;
            
            window.LoadGraph();
            window.Focus();
        }

        private static bool TryGetOpenedGraphEditorWindowWithContent<T>(Object serializationRoot, string propertyPath, out T editorWindow) where T : GraphEditorWindow
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

        private static T OpenNewGraphEditorWindow<T>() where T : GraphEditorWindow
        {
            return CreateWindow<T>(typeof(T));
        }

        private static IEnumerable<T> GetOpenedGraphEditorWindows<T>() where T : GraphEditorWindow
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }
        
        
        private bool TryGetSerializedProperty(Object serializationRoot, string graphPropertyPath, out SerializedProperty serializedProperty)
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
    }
}

