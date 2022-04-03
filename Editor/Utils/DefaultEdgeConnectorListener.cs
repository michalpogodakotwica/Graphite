using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.Editor.Utils
{
    /// Mimics Unity's default edge connector listener behaviour.
    public class DefaultEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly GraphViewChange _graphViewChange;
        private readonly List<Edge> _edgesToCreate;
        private readonly List<GraphElement> _edgesToDelete;

        public DefaultEdgeConnectorListener()
        {
            _edgesToCreate = new List<Edge>();
            _edgesToDelete = new List<GraphElement>();
            _graphViewChange.edgesToCreate = _edgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            _edgesToCreate.Clear();
            _edgesToCreate.Add(edge);
            _edgesToDelete.Clear();
            
            if (edge.input.capacity == Port.Capacity.Single)
            {
                foreach (var connection in edge.input.connections)
                {
                    if (connection != edge)
                        _edgesToDelete.Add(connection);
                }
            }

            if (edge.output.capacity == Port.Capacity.Single)
            {
                foreach (var connection in edge.output.connections)
                {
                    if (connection != edge)
                        _edgesToDelete.Add(connection);
                }
            }

            if (_edgesToDelete.Count > 0)
                graphView.DeleteElements(_edgesToDelete);
            
            var edgesToCreate = _edgesToCreate;
            if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(_graphViewChange).edgesToCreate;
            
            foreach (var edgeToCreate in edgesToCreate)
            {
                graphView.AddElement(edgeToCreate);
                edge.input.Connect(edgeToCreate);
                edge.output.Connect(edgeToCreate);
            }
        }
    }
}