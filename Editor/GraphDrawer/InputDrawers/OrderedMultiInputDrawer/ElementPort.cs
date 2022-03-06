using System;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Editor.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Graphite.Editor.GraphDrawer.InputDrawers.OrderedMultiInputDrawer
{
    public class ElementPort : Port
    {
        private readonly OrderedMultiInputDrawer _inputDrawer;

        private int _portIndex;

        public int PortIndex
        {
            get => _portIndex;
            internal set
            {
                _portIndex = value;
                portName = _inputDrawer.ElementName + " " + (value + 1);
            }
        }

        private ElementPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type, OrderedMultiInputDrawer inputDrawer) 
            : base(portOrientation, portDirection, portCapacity, type)
        {
            _inputDrawer = inputDrawer;
        }

        public override void Connect(Edge edge)
        {
            var input = _inputDrawer.GraphViewSettings.DisplaySettings.OutputsOnRight ? edge.input : edge.output;
            var output = _inputDrawer.GraphViewSettings.DisplaySettings.OutputsOnRight ? edge.output : edge.input;
            
            if (input.node == null && output.node is NodeDrawer outputNodeView)
            {
                var outputDrawer = outputNodeView.OutputMap[output];
                
                output.Disconnect(edge);
                input.Disconnect(edge);
                outputDrawer.Parent.Parent.RemoveElement(edge);
                
                _inputDrawer.OnElementPortConnected(PortIndex, outputDrawer);
                
                base.Connect(edge);
            }
            else
                base.Connect(edge);
        }

        public static ElementPort Create<TEdge>(
            Orientation orientation,
            Direction direction,
            Capacity capacity,
            Type type,
            OrderedMultiInputDrawer inputDrawer)
            where TEdge : Edge, new()
        {
            var connectorListener = new DefaultEdgeConnectorListener();
            var port = new ElementPort(orientation, direction, capacity, type, inputDrawer)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(connectorListener)
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }
    }
}