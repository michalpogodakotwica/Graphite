using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Editor.GraphDrawer.OutputDrawers;
using Graphite.Runtime.Ports;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Graphite.Editor.GraphDrawer.InputDrawers.SingleInputDrawer
{
    [CustomInputDrawer(typeof(SingleInput))]
    public class SingleInputDrawer : InputDrawer
    {
        private readonly Port _port;
        private readonly SingleInput _content;
        
        public SingleInputDrawer(SingleInput content, NodeDrawer parent, SerializedProperty inputProperty) 
            : base(content, parent, inputProperty)
        {
            _content = content;
            _port = Port.Create<Edge>(Orientation.Horizontal, PortDirection, Port.Capacity.Single, _content.Type);  
            
            foreach (var styleSheet in Parent.GraphViewSettings.DisplaySettings.GraphStyleSheets)
            {
                _port.styleSheets.Add(styleSheet);
            }

            _port.portName = inputProperty.displayName;
        }

        public override void DrawPorts()
        { 
            Parent.AddInputPort(_port, this);
            PortContainer.Add(_port);
        }

        public override void ClearPorts() 
        {
            Parent.RemoveInputPort(_port);
            PortContainer.Remove(_port);
        }
        
        public override void DrawConnections()
        {
            var connection = _content.GetConnection();
            if (connection != null)
            {
                DrawConnection(Parent.Parent.OutputsMapping[connection]);   
            }
        }

        private void DrawConnection(OutputDrawer outputDrawer)
        {
            var edge = _port.ConnectTo<Edge>(outputDrawer.Port);
            Parent.Parent.AddElement(edge);
        }

        public override bool TryToConnect(OutputDrawer outputDrawer, Edge edge)
        {
            InputProperty.FindPropertyRelative("Connection").managedReferenceValue = outputDrawer.Content;
            return true;
        }
        
        public override bool TryToDisconnect(OutputDrawer outputDrawer, Edge edge)
        {
            InputProperty.FindPropertyRelative("Connection").managedReferenceValue = null;
            return true;
        }
    }
}