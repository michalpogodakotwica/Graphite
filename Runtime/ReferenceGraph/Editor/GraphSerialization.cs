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

namespace ReferenceGraph.Editor
{
    public class GraphSerialization : IGraphSerializationBackend
    {
        private void ModifyWithUndo(GraphDrawer graphDrawer,
            Action applyModification)
        {
            var graphNodes = graphDrawer.GraphProperty.FindPropertyRelative("_nodes");
            graphNodes.serializedObject.Update();
            Undo.RecordObject(graphNodes.serializedObject.targetObject, $"Graph changes");
            applyModification();
            EditorUtility.SetDirty(graphNodes.serializedObject.targetObject);
            graphNodes.serializedObject.ApplyModifiedProperties();
        }

        public IEnumerable<(SerializedProperty, com.michalpogodakotwica.graphite.INode)> GetAllNodes(
            GraphDrawer graphDrawer)
        {
            var graphNodes = graphDrawer.GraphProperty.FindPropertyRelative("_nodes");
            graphNodes.serializedObject.Update();

            for (var i = 0; i < graphNodes.arraySize; i++)
            {
                var nodeProperty = graphNodes.GetArrayElementAtIndex(i);
                var node = (INode)nodeProperty.GetValue();
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

        public IEnumerable<(SerializedProperty, com.michalpogodakotwica.graphite.INode)> AddNodes(
            GraphDrawer graphDrawer,
            List<com.michalpogodakotwica.graphite.INode> nodesToAdd)
        {
            List<(SerializedProperty, com.michalpogodakotwica.graphite.INode)> addedNodes = default;
            ModifyWithUndo(graphDrawer, () => { addedNodes = AddNodesWithoutUndo(graphDrawer, nodesToAdd); });
            return addedNodes;
        }

        private List<(SerializedProperty, com.michalpogodakotwica.graphite.INode)> AddNodesWithoutUndo(
            GraphDrawer graphDrawer,
            List<com.michalpogodakotwica.graphite.INode> nodesToAdd)
        {
            var graphNodes = graphDrawer.GraphProperty.FindPropertyRelative("_nodes");
            var result = new List<(SerializedProperty, com.michalpogodakotwica.graphite.INode)>();

            foreach (var node in nodesToAdd)
            {
                var size = graphNodes.arraySize;
                graphNodes.arraySize++;
                graphNodes.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var casted = (INode) node;
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

                    var input = graphDrawer.Settings.DisplaySettings.OutputsOnRight ? edge.input : edge.output;
                    var output = graphDrawer.Settings.DisplaySettings.OutputsOnRight ? edge.output : edge.input;

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
                    var input = graphDrawer.Settings.DisplaySettings.OutputsOnRight ? edge.input : edge.output;
                    var output = graphDrawer.Settings.DisplaySettings.OutputsOnRight ? edge.output : edge.input;

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
            var graphNodes = graphDrawer.GraphProperty.FindPropertyRelative("_nodes");

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
                ReassignProperties(graphDrawer, indexesToRemove.Min());
        }

        private void ReassignProperties(GraphDrawer graphDrawer,
            int startingIndex = 0)
        {
            var graphNodes = graphDrawer.GraphProperty.FindPropertyRelative("_nodes");
            for (var index = startingIndex; index < graphDrawer.NodeDrawers.Count; index++)
            {
                var node = graphDrawer.NodeDrawers[index];
                node.ReassignProperty(graphNodes.GetArrayElementAtIndex(index));
            }
        }
    }
}