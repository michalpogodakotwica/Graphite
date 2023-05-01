using System;
using System.Collections.Generic;
using System.Linq;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.Editor.SerializationBackend;
using com.michalpogodakotwica.graphite.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.GuidGraph.Editor
{
    public class GraphSerialization : IGraphSerializationBackend
    {
        private void ModifyWithUndo(GraphDrawer graphDrawer, Action applyModification)
        {
            var serializedObject = new SerializedObject(graphDrawer.EditorWindow);
            Undo.RecordObject(serializedObject.targetObject, $"Graph changes");
            serializedObject.Update();
            
            applyModification();
            EditorUtility.SetDirty(serializedObject.targetObject);
            
            serializedObject.ApplyModifiedProperties();
        }

        public IEnumerable<(SerializedProperty, INode)> GetAllNodes(
            GraphDrawer graphDrawer)
        {
            var serializedObject = new SerializedObject(graphDrawer.EditorWindow);
            var graphNodes = serializedObject.FindProperty("_nodes");
            graphNodes.serializedObject.Update();

            for (var i = 0; i < graphNodes.arraySize; i++)
            {
                var nodeProperty = graphNodes.GetArrayElementAtIndex(i);
                var node = (global::com.michalpogodakotwica.graphite.GuidGraph.Runtime.INode)nodeProperty.GetValue();
                node.Initialize();
                yield return (nodeProperty, node);
            }
        }

        public GraphViewChange SerializeGraphViewChange(
            GraphDrawer graphDrawer, GraphViewChange viewChanges)
        {
            GraphViewChange graphViewChange = default;
            ModifyWithUndo(graphDrawer,
                () => { graphViewChange = GraphViewChangeWithoutUndo(graphDrawer, viewChanges); });
            return graphViewChange;
        }

        public IEnumerable<(SerializedProperty, INode)> AddNodes(
            GraphDrawer graphDrawer,
            List<INode> nodesToAdd)
        {
            List<(SerializedProperty, INode)> addedNodes = default;
            ModifyWithUndo(graphDrawer, () => { addedNodes = AddNodesWithoutUndo(graphDrawer, nodesToAdd); });
            return addedNodes;
        }

        private List<(SerializedProperty, INode)> AddNodesWithoutUndo(
            GraphDrawer graphDrawer,
            List<INode> nodesToAdd)
        {
            var serializedObject = new SerializedObject(graphDrawer.EditorWindow);
            var graphNodes = serializedObject.FindProperty("_nodes");
            var result = new List<(SerializedProperty, INode)>();

            foreach (var node in nodesToAdd)
            {
                var size = graphNodes.arraySize;
                graphNodes.arraySize++;
                graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var casted = (global::com.michalpogodakotwica.graphite.GuidGraph.Runtime.INode) node;
                casted.Initialize();
                
                graphNodes.GetArrayElementAtIndex(size).managedReferenceValue = node;
                graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var nodeProperty = graphNodes.GetArrayElementAtIndex(size);

                result.Add((nodeProperty, node));

                EditorUtility.SetDirty(graphNodes.serializedObject.targetObject);
            }

            return result;
        }

        private GraphViewChange GraphViewChangeWithoutUndo(
            GraphDrawer graphDrawer, GraphViewChange viewChanges)
        {
            if (viewChanges.movedElements != null)
                MoveNodeViews(graphDrawer, ref viewChanges.movedElements, viewChanges.moveDelta);

            if (viewChanges.elementsToRemove != null)
                RemoveEdges(graphDrawer, ref viewChanges.elementsToRemove);

            if (viewChanges.edgesToCreate != null)
                CreateEdges(graphDrawer, ref viewChanges.edgesToCreate);

            if (viewChanges.elementsToRemove != null)
                RemoveNodeViews(graphDrawer, ref viewChanges.elementsToRemove);

            return viewChanges;
        }

        private void MoveNodeViews(GraphDrawer graphDrawer,
            ref List<GraphElement> elementsToMove, Vector2 movementDelta)
        {
            elementsToMove.RemoveAll(m => m is NodeDrawer nodeView && !nodeView.TryToMove(movementDelta));
        }

        private void RemoveEdges(GraphDrawer graphDrawer,
            ref List<GraphElement> elementsToRemove)
        {
            elementsToRemove.RemoveAll(element =>
                {
                    if (element is not Edge edge)
                        return false;

                    var outputsOnRight = !graphDrawer.Settings.DisplaySettings.ReverseConnectionFlow;
                    var input = outputsOnRight ? edge.input : edge.output;
                    var output = outputsOnRight ? edge.output : edge.input;

                    return input.node is NodeDrawer inputNodeView &&
                           output.node is NodeDrawer outputNodeView &&
                           !inputNodeView.InputMap[input]
                               .TryToDisconnect(outputNodeView.OutputMap[output], edge);
                }
            );
        }

        private void CreateEdges(GraphDrawer graphDrawer,
            ref List<Edge> edgesToCreate)
        {
            edgesToCreate.RemoveAll(edge =>
                {
                    var outputsOnRight = !graphDrawer.Settings.DisplaySettings.ReverseConnectionFlow;
                    var input = outputsOnRight ? edge.input : edge.output;
                    var output = outputsOnRight ? edge.output : edge.input;

                    return input.node is NodeDrawer inputNodeView &&
                           output.node is NodeDrawer outputNodeView &&
                           !inputNodeView.InputMap[input]
                               .TryToConnect(outputNodeView.OutputMap[output], edge);
                }
            );
        }

        private void RemoveNodeViews(GraphDrawer graphDrawer,
            ref List<GraphElement> elementsToRemove)
        {
            var serializedObject = new SerializedObject(graphDrawer.EditorWindow);
            var graphNodes = serializedObject.FindProperty("_nodes");
            var indexesToRemove = new List<int>();

            foreach (var element in elementsToRemove)
            {
                if (element is not NodeDrawer nodeView)
                    continue;

                int index;
                for (index = 0; index < graphDrawer.NodeDrawers.Count; index++)
                {
                    if (graphDrawer.NodeDrawers[index] != nodeView)
                        continue;

                    indexesToRemove.Add(index);
                    break;
                }
            }

            foreach (var nodeIndex in indexesToRemove.OrderByDescending(n => n))
            {
                graphNodes.GetArrayElementAtIndex(nodeIndex).SetValue(null);
                graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                graphNodes.DeleteArrayElementAtIndex(nodeIndex);
                graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            if (indexesToRemove.Count > 0)
                ReassignProperties(graphDrawer, indexesToRemove);
        }

        private void ReassignProperties(GraphDrawer graphDrawer, List<int> indexesToRemove)
        {
            var graphNodes = graphDrawer.GraphProperty.FindPropertyRelative("_nodes");
            var nodesKept = Enumerable.Range(0, graphDrawer.NodeDrawers.Count).Except(indexesToRemove).ToArray();
            
            for (var newIndex = indexesToRemove.Min(); newIndex < nodesKept.Length; newIndex++)
            {
                var oldIndex = nodesKept[newIndex];
                var node = graphDrawer.NodeDrawers[oldIndex];
                node.ReassignProperty(graphNodes.GetArrayElementAtIndex(newIndex));
            }
        }
    }
}