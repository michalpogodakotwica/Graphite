using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Editor.GraphDrawer.OutputDrawers;
using Graphite.Editor.Settings;
using Graphite.Editor.Utils;
using Graphite.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphite.Editor.GraphDrawer
{
    public class GraphDrawer : GraphView, IDisposable
    {
        public readonly GraphViewSettings Settings;
        public readonly GraphEditorWindow EditorWindow;
        
        private readonly SerializedProperty _graphNodes;
        private readonly Graph _graph;

        private readonly CreateNodeSearchWindowProvider.CreateNodeSearchWindowProvider _createNodeSearchWindowProviderProvider;
        
        public readonly Dictionary<IOutput, OutputDrawer> OutputsMapping = new();
        private readonly List<NodeDrawer> _nodeViewsInSerializedArrayOrder = new();

        public GraphDrawer(SerializedProperty graphNodes, Graph graph, GraphEditorWindow editorWindow, GraphViewSettings settings)
        {
            _graphNodes = graphNodes;
            _graph = graph;
            Settings = settings;

            EditorWindow = editorWindow;
            
            _createNodeSearchWindowProviderProvider = ScriptableObject.CreateInstance<CreateNodeSearchWindowProvider.CreateNodeSearchWindowProvider>();
            _createNodeSearchWindowProviderProvider.Initialize(this);
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            SetupZoom(
                settings.DisplaySettings.ScaleSettings.MinScale,
                settings.DisplaySettings.ScaleSettings.MaxScale,
                settings.DisplaySettings.ScaleSettings.ScaleStep,
                settings.DisplaySettings.ScaleSettings.ReferenceScale
            );

            foreach (var styleSheet in settings.DisplaySettings.GraphStyleSheets)
            {
                styleSheets.Add(styleSheet);
            }

            if (settings.MiniMapSettings.EnableMinimap)
            {
                Minimap.AddMinimapManipulator(this);
            }

            if (settings.DisplaySettings.EnableGrid)
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
            graphViewChanged += InternalGraphViewChangeWithUndo;

            if (Settings.CopyPasteHandler == null) 
                return;
            
            serializeGraphElements += elements => Settings.CopyPasteHandler.SerializeGraphElementsCallback(this, elements);
            canPasteSerializedData += data => Settings.CopyPasteHandler.CanPasteSerializedDataCallback(this, data);
            unserializeAndPaste += (operationName, data) => Settings.CopyPasteHandler.UnserializeAndPasteCallback(this, operationName, data);
        }

        private void RemoveListeners()
        {
            Undo.undoRedoPerformed -= RedrawGraph;
            graphViewChanged -= InternalGraphViewChangeWithUndo;
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

        private void ClearGraph()
        {
            foreach (var nodeView in _nodeViewsInSerializedArrayOrder)
            {
                nodeView.ClearNode();
                RemoveElement(nodeView);
            }

            _nodeViewsInSerializedArrayOrder.Clear();
            
            foreach (var edge in edges)
                RemoveElement(edge);
        }
        
        protected virtual void DrawGraph()
        {
            _graphNodes.serializedObject.Update();
            
            var i = 0;
            foreach (var node in _graph)
            {
                var nodeProperty = _graphNodes.GetArrayElementAtIndex(i);
                var drawer = CreateNodeDrawer(node, nodeProperty);
                AddNodeDrawer(drawer);
                i++;
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
            _nodeViewsInSerializedArrayOrder.Remove(drawer);
        }

        protected  virtual void AddNodeDrawer(NodeDrawer drawer)
        {
            drawer.AddNode();
            AddElement(drawer);
            _nodeViewsInSerializedArrayOrder.Add(drawer);
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

        public void ModifyWithUndo(Action applyModification)
        {
            _graphNodes.serializedObject.Update();
            Undo.RecordObject(_graphNodes.serializedObject.targetObject, $"Graph changes");
            applyModification();
            EditorUtility.SetDirty(_graphNodes.serializedObject.targetObject);
            _graphNodes.serializedObject.ApplyModifiedProperties();
        }

        private GraphViewChange InternalGraphViewChangeWithUndo(GraphViewChange viewChanges)
        {
            GraphViewChange graphViewChange = default;
            ModifyWithUndo(() =>
            {
                graphViewChange = InternalGraphViewChange(viewChanges);
            });
            return graphViewChange;
        }

        public void AddNodes(List<INode> nodesToAdd)
        {
            ClearSelection();

            var newViews = new List<NodeDrawer>();
            foreach (var node in nodesToAdd)
            {
                var size = _graphNodes.arraySize;
                _graphNodes.arraySize++;
                _graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                
                _graphNodes.GetArrayElementAtIndex(size).managedReferenceValue = node;
                _graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var nodeProperty = _graphNodes.GetArrayElementAtIndex(size);
                
                var drawer = CreateNodeDrawer(node, nodeProperty);
                AddNodeDrawer(drawer);
                EditorUtility.SetDirty(_graphNodes.serializedObject.targetObject);
                
                newViews.Add(drawer);
            }

            foreach (var drawer in newViews)
            {
                drawer.DrawConnections();
            }

            foreach (var drawer in newViews)
            {
                AddToSelection(drawer);
            }
        }
        
        protected virtual GraphViewChange InternalGraphViewChange(GraphViewChange viewChanges)
        {
            if(viewChanges.movedElements != null)
                MoveNodeViews(ref viewChanges.movedElements, viewChanges.moveDelta);
            
            if (viewChanges.elementsToRemove != null)
                RemoveEdges(ref viewChanges.elementsToRemove);

            if (viewChanges.edgesToCreate != null)
                CreateEdges(ref viewChanges.edgesToCreate);

            if (viewChanges.elementsToRemove != null)
                RemoveNodeViews(ref viewChanges.elementsToRemove);
            
            return viewChanges;
        }
        
        protected virtual void MoveNodeViews(ref List<GraphElement> elementsToMove, Vector2 movementDelta)
        {
            elementsToMove.RemoveAll(m => m is NodeDrawer nodeView && !nodeView.TryToMove(movementDelta));
        }

        protected virtual void RemoveEdges(ref List<GraphElement> elementsToRemove)
        {
            elementsToRemove.RemoveAll(element =>
                {
                    if (element is not Edge edge)
                        return false;
                    
                    var input = Settings.DisplaySettings.OutputsOnRight ? edge.input : edge.output;
                    var output = Settings.DisplaySettings.OutputsOnRight ? edge.output : edge.input;

                    return input.node is NodeDrawer inputNodeView &&
                           output.node is NodeDrawer outputNodeView &&
                           !inputNodeView.InputMap[input]
                               .TryToDisconnect(outputNodeView.OutputMap[output], edge);
                }
            );
        }

        protected virtual void RemoveNodeViews(ref List<GraphElement> elementsToRemove)
        {
            var indexesToRemove = new List<int>();
            
            foreach (var element in elementsToRemove)
            {
                if (element is not NodeDrawer nodeView)
                    continue;
                
                int index;
                for (index = 0; index < _nodeViewsInSerializedArrayOrder.Count; index++)
                {
                    if (_nodeViewsInSerializedArrayOrder[index] != nodeView) 
                        continue;
                    
                    indexesToRemove.Add(index);
                    RemoveNodeDrawer(nodeView);
                    break;
                }
            }

            foreach (var nodeIndex in indexesToRemove.OrderByDescending(n => n))
            {
                _graphNodes.GetArrayElementAtIndex(nodeIndex).SetValue(null);
                _graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        
                _graphNodes.DeleteArrayElementAtIndex(nodeIndex);
                _graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            if (indexesToRemove.Count > 0)
                ReassignProperties(indexesToRemove.Min());
        }
        
        protected virtual void CreateEdges(ref List<Edge> edgesToCreate)
        {
            edgesToCreate.RemoveAll(edge =>
                {
                    var input = Settings.DisplaySettings.OutputsOnRight ? edge.input : edge.output;
                    var output = Settings.DisplaySettings.OutputsOnRight ? edge.output : edge.input;

                    return input.node is NodeDrawer inputNodeView &&
                           output.node is NodeDrawer outputNodeView &&
                           !inputNodeView.InputMap[input]
                               .TryToConnect(outputNodeView.OutputMap[output], edge);
                }
            );
        }
        
        private void ReassignProperties(int startingIndex = 0)
        {
            for (var index = startingIndex; index < _nodeViewsInSerializedArrayOrder.Count; index++)
            {
                var node = _nodeViewsInSerializedArrayOrder[index];
                node.ReassignProperty(_graphNodes.GetArrayElementAtIndex(index));
            }
        }
    }
}