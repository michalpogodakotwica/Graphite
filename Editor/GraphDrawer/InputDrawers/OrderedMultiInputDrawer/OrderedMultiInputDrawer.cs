using System.Collections.Generic;
using System.Linq;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Editor.GraphDrawer.OutputDrawers;
using Graphite.Runtime;
using Graphite.Runtime.Ports;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Graphite.Editor.GraphDrawer.InputDrawers.OrderedMultiInputDrawer
{
    [CustomInputDrawer(typeof(MultiInput))]
    public class OrderedMultiInputDrawer : InputDrawer
    {
        private readonly MultiInput _content;
        
        private readonly List<ElementPort> _ports = new();
        private Port _addPort;
        
        private SerializedProperty _connectionsProperty;

        public OrderedMultiInputDrawer(MultiInput content, NodeDrawer parent, SerializedProperty inputProperty) : base(content, parent, inputProperty)
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
            foreach (var unused in ((IInput)_content).Connections)
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
            var currentConnections = ((IInput)_content).Connections.ToArray();
            for (var index = 0; index < currentConnections.Length; index++)
            {
                var connection = currentConnections[index];
                DrawConnection(_ports[index], Parent.Parent.OutputsMapping[connection]);
            }
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
        
        private void AssignToPropertyAtIndex(int index, Output value)
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
}