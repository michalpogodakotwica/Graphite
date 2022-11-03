using System;
using System.Collections.Generic;
using System.Linq;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.Editor.SerializationBackend;
using com.michalpogodakotwica.graphite.Editor.Settings;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer
{
    [Serializable]
    public abstract class GraphDrawer : GraphView, IDisposable
    {
        public readonly GraphViewSettings Settings;
        public readonly GraphEditorWindow EditorWindow;

        public SerializedProperty GraphProperty => EditorWindow.GraphProperty;

        private readonly CreateNodeSearchWindowProvider.CreateNodeSearchWindowProvider _createNodeSearchWindowProviderProvider;
        
        public readonly List<NodeDrawer> NodeDrawers = new ();
        protected IGraphSerializationBackend GraphSerializationBackend;

        public GraphDrawer(GraphEditorWindow editorWindow)
        {
            Settings = editorWindow.ViewSettings;

            EditorWindow = editorWindow;
            
            _createNodeSearchWindowProviderProvider = ScriptableObject.CreateInstance<CreateNodeSearchWindowProvider.CreateNodeSearchWindowProvider>();
            _createNodeSearchWindowProviderProvider.Initialize(this);
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupZoom(
                Settings.DisplaySettings.ScaleSettings.MinScale,
                Settings.DisplaySettings.ScaleSettings.MaxScale,
                Settings.DisplaySettings.ScaleSettings.ScaleStep,
                Settings.DisplaySettings.ScaleSettings.ReferenceScale
            );

            foreach (var styleSheet in Settings.DisplaySettings.GraphStyleSheets)
            {
                styleSheets.Add(styleSheet);
            }

            if (Settings.MiniMapSettings.EnableMinimap)
            {
                Minimap.AddMinimapManipulator(this);
            }

            if (Settings.DisplaySettings.EnableGrid)
            {
                var grid = new GridBackground();
                Insert(0, grid);
            }
            
            AddListeners();
        }

        public void Dispose()
        {
            RemoveListeners();
            ClearGraph();
        }

        private void AddListeners()
        {
            Undo.undoRedoPerformed += RedrawGraph;
            nodeCreationRequest += OnNodeCreationRequest; 
            graphViewChanged += InternalGraphViewChange;

            if (Settings.CopyPasteHandler == null) 
                return;
            
            serializeGraphElements += elements => Settings.CopyPasteHandler.SerializeGraphElementsCallback(this, elements);
            canPasteSerializedData += data => Settings.CopyPasteHandler.CanPasteSerializedDataCallback(this, data);
            unserializeAndPaste += (operationName, data) => Settings.CopyPasteHandler.UnserializeAndPasteCallback(this, operationName, data);
        }

        private void RemoveListeners()
        {
            Undo.undoRedoPerformed -= RedrawGraph;
            graphViewChanged -= InternalGraphViewChange;
            nodeCreationRequest -= OnNodeCreationRequest;

            if (Settings.CopyPasteHandler == null) 
                return;
            
            serializeGraphElements -= elements => Settings.CopyPasteHandler.SerializeGraphElementsCallback(this, elements);
            canPasteSerializedData -= data => Settings.CopyPasteHandler.CanPasteSerializedDataCallback(this, data);
            unserializeAndPaste -= (operationName, data) => Settings.CopyPasteHandler.UnserializeAndPasteCallback(this, operationName, data);
        }
        
        public virtual void RedrawGraph()
        {
            ClearGraph();
            DrawGraph();
        }
        
        public void AddNodes(List<INode> nodesToAdd)
        {
            if (nodesToAdd == null || nodesToAdd.Count == 0)
            {
                return;
            }
            
            ClearSelection();
            
            var addedNodes = GraphSerializationBackend.AddNodes(this, nodesToAdd);
            var drawers = new List<NodeDrawer>();
            
            foreach (var (nodeSerializedProperty, node) in addedNodes)
            {
                var drawer = CreateNodeDrawer(node, nodeSerializedProperty);
                AddNodeDrawer(drawer);
                AddToSelection(drawer);
                drawers.Add(drawer);
            }

            foreach (var drawer in drawers)
                drawer.DrawConnections();
        }
        
        private GraphViewChange InternalGraphViewChange(GraphViewChange graphViewChange)
        {
            graphViewChange = GraphSerializationBackend.SerializeGraphViewChange(this, graphViewChange);

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var elementToRemove in graphViewChange.elementsToRemove)
                {
                    if (elementToRemove is NodeDrawer nodeDrawer)
                        RemoveNodeDrawer(nodeDrawer);
                }
            }

            return graphViewChange;
        }

        private void ClearGraph()
        {
            foreach (var nodeView in NodeDrawers)
            {
                nodeView.ClearNode();
                RemoveElement(nodeView);
            }

            NodeDrawers.Clear();
            
            foreach (var edge in edges)
                RemoveElement(edge);
        }
        
        protected virtual void DrawGraph()
        {
            foreach (var node in GraphSerializationBackend.GetAllNodes(this))
            {
                var drawer = CreateNodeDrawer(node.Item2, node.Item1);
                AddNodeDrawer(drawer);
            }
            
            foreach (var node in nodes.Where(n => n is NodeDrawer).Cast<NodeDrawer>())
                node.DrawConnections();
        }

        private NodeDrawer CreateNodeDrawer(INode node, SerializedProperty property)
        {
            var settings = NodeViewSettings.Default(node);
            var drawer = Settings.NodeDrawerTypeMapping.GetDrawerForType(node.GetType());
            return (NodeDrawer)Activator.CreateInstance(drawer, node, this, property, settings);
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return Settings.CompatiblePortsProvider.GetCompatiblePorts(this, startPort, nodeAdapter);
        }
        
        protected virtual void RemoveNodeDrawer(NodeDrawer drawer)
        {
            drawer.ClearNode();
            RemoveElement(drawer);
            NodeDrawers.Remove(drawer);
        }

        protected virtual void AddNodeDrawer(NodeDrawer drawer)
        {
            drawer.AddNode();
            AddElement(drawer);
            NodeDrawers.Add(drawer);
        }

        protected virtual void OnNodeCreationRequest(NodeCreationContext context)
        {
            var searchWindowContext = new SearchWindowContext(context.screenMousePosition);
            SearchWindow.Open(searchWindowContext, _createNodeSearchWindowProviderProvider);
        }
        
        public Vector2 ScreenToGraphPosition(Vector2 screenPosition)
        {
            var windowRoot = EditorWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, screenPosition - EditorWindow.position.position);
            return contentViewContainer.WorldToLocal(windowMousePosition);
        }
    }
}