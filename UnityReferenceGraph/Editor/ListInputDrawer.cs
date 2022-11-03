using System;
using System.Collections.Generic;
using System.Linq;
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.Editor.Utils;
using com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Editor
{
    [CustomInputDrawer(typeof(IListInput))]
    public class ListInputDrawer : global::com.michalpogodakotwica.graphite.Editor.GraphDrawer.InputDrawer
    {
        private readonly IListInput _content;
        
        private readonly List<ElementPort> _ports = new();
        private Port _addPort;
        
        private SerializedProperty _connectionsProperty;

        public ListInputDrawer(IListInput content, NodeDrawer parent, SerializedProperty inputProperty) : base(content, parent, inputProperty)
        {
            _content = content;
            _connectionsProperty = inputProperty.FindPropertyRelative("Connections");
        }

        public override void ReassignProperty(SerializedProperty inputProperty)
        {
            base.ReassignProperty(inputProperty);
            _connectionsProperty = InputProperty.FindPropertyRelative("Connections");
        }

        public override void DrawPorts()
        {
            foreach (var unused in _content.Connections)
            {
                var port = ElementPort.Create<Edge>(Orientation.Horizontal, PortDirection, Port.Capacity.Single, _content.Type, this);

                foreach (var styleSheet in Parent.GraphViewSettings.DisplaySettings.GraphStyleSheets)
                {
                    port.styleSheets.Add(styleSheet);
                }
                
                _ports.Add(port);
                Parent.AddInputPort(port, this);
                PortContainer.Add(port);
            }
            
            ReassignPortViewIndexes();
            _addPort = Port.Create<Edge>(Orientation.Horizontal, PortDirection, Port.Capacity.Single, _content.Type);

            foreach (var styleSheet in Parent.GraphViewSettings.DisplaySettings.GraphStyleSheets)
            {
                _addPort.styleSheets.Add(styleSheet);
            }
            
            _addPort.portName = "New " + ElementName;
            Parent.AddInputPort(_addPort, this);
            PortContainer.Add(_addPort);
        }
        
        public override void DrawConnections()
        {
            var currentConnections = _content.Connections.ToArray();
            var minIndex = -1;
            for (var index = currentConnections.Length - 1; index >= 0; index--)
            {
                var connection = currentConnections[index];
                
                if (((UnityReferenceGraphDrawer)Parent.Parent).OutputsMapping.TryGetValue(connection, out var drawer))
                {
                    DrawConnection(_ports[index], drawer);
                }
                else
                {
                    minIndex = index;
                    RemovePortView(index);
                    RemoveFromPropertyAtIndex(index);
                }
            }

            if (minIndex == -1)
                return;
            
            ReassignPortViewIndexes(0);
            Parent.RefreshPorts();
        }

        private void DrawConnection(Port port, OutputDrawer outputDrawer)
        {
            var edge = port.ConnectTo<Edge>(outputDrawer.Port);
            Parent.Parent.AddElement(edge);
        }

        public override void ClearPorts() 
        {
            foreach (var port in _ports)
            {
                Parent.RemoveInputPort(port);
                PortContainer.Remove(port);
            }
            _ports.Clear();
            
            Parent.RemoveInputPort(_addPort);
            PortContainer.Remove(_addPort);
        }

        public override bool TryToConnect(OutputDrawer outputDrawer, Edge edge)
        {
            var input = GraphViewSettings.DisplaySettings.OutputsOnRight ? edge.input : edge.output;
            
            if (input == _addPort)
            {
                OnAddPortConnected(outputDrawer);
                return false;
            }
            
            return true;
        }
        
        public override bool TryToDisconnect(OutputDrawer outputDrawer, Edge edge)
        {
            var input = GraphViewSettings.DisplaySettings.OutputsOnRight 
                ? (ElementPort)edge.input 
                : (ElementPort)edge.output;
            OnElementPortDisconnected(input.PortIndex);
            return true;
        }
        
        public void OnElementPortConnected(int portIndex, OutputDrawer outputDrawer)
        {
            UpdateConnectionsSerializedObject();
            
            InsertPropertyElementAtIndex(portIndex);
            AssignToPropertyAtIndex(portIndex, outputDrawer.Content);
            var port = InsertPortView(portIndex);
            
            ReassignPortViewIndexes(portIndex + 1);
            
            Parent.RefreshPorts();
            DrawConnection(port, outputDrawer);
        }
        
        public void OnElementPortDisconnected(int portIndex)
        {
            UpdateConnectionsSerializedObject();
            RemovePorts(new []{portIndex});
            Parent.RefreshPorts();
        }

        private void OnAddPortConnected(OutputDrawer outputDrawer)
        {
            UpdateConnectionsSerializedObject();
            AppendProperty();
            AssignToPropertyAtIndex(_ports.Count, outputDrawer.Content);
            var port = AddPortView();
            Parent.RefreshPorts();
            
            DrawConnection(port, outputDrawer);
        }
        
        private void RemovePorts(IEnumerable<int> indexesToRemove)
        {
            var lowestIndex = _ports.Count;
            foreach (var indexToRemove in indexesToRemove.OrderByDescending(i => i))
            {
                RemovePortView(indexToRemove);
                RemoveFromPropertyAtIndex(indexToRemove);
                lowestIndex = indexToRemove;
            }
            
            ReassignPortViewIndexes(lowestIndex);
        }

        private void ReassignPortViewIndexes(int startingIndex = 0)
        {
            for (var i = startingIndex; i < _ports.Count; i++)
            {
                _ports[i].PortIndex = i;
            }
        }
        
        private void UpdateConnectionsSerializedObject()
        {
            _connectionsProperty.serializedObject.Update();
        }
        
        private void AppendProperty()
        {
            _connectionsProperty.arraySize++;
            _connectionsProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private void InsertPropertyElementAtIndex(int index)
        {
            _connectionsProperty.InsertArrayElementAtIndex(index);
            _connectionsProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private void AssignToPropertyAtIndex(int index, IOutput value)
        {
            _connectionsProperty.GetArrayElementAtIndex(index).managedReferenceValue = value;
            _connectionsProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void RemoveFromPropertyAtIndex(int index)
        {
            _connectionsProperty.DeleteArrayElementAtIndex(index);
            _connectionsProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private ElementPort AddPortView()
        {
            var elementIndex = PortContainer.IndexOf(_addPort);
            return InsertPortViewAtElementIndex(_ports.Count, elementIndex);
        }

        private ElementPort InsertPortView(int portIndex)
        {
            var elementIndex = PortContainer.IndexOf(_addPort);
            return InsertPortViewAtElementIndex(portIndex, elementIndex - _ports.Count + portIndex);
        }
        
        private ElementPort InsertPortViewAtElementIndex(int portIndex, int elementIndex)
        {
            var port = ElementPort.Create<Edge>(Orientation.Horizontal, PortDirection, Port.Capacity.Single, _content.Type, this);
            
            foreach (var styleSheet in Parent.GraphViewSettings.DisplaySettings.GraphStyleSheets)
            {
                port.styleSheets.Add(styleSheet);
            }
            
            port.PortIndex = portIndex;
            
            _ports.Insert(portIndex, port);
            Parent.AddInputPort(port, this);
            PortContainer.Insert(elementIndex, port);
            return port;
        }
        
        private void RemovePortView(int index)
        {
            var portToRemove = _ports[index];
            Parent.RemoveInputPort(portToRemove);     
            PortContainer.Remove(portToRemove);
            
            _ports.RemoveAt(index);
        }
        
        public string ElementName => InputProperty.displayName.TrimEnd('s');
    }
    
    public class ElementPort : Port
    {
        private readonly ListInputDrawer _inputDrawer;

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

        private ElementPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type, ListInputDrawer inputDrawer) 
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
            ListInputDrawer inputDrawer)
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