using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Editor
{
    [CustomInputDrawer(typeof(ISingleInput))]
    public class InputDrawer : global::com.michalpogodakotwica.graphite.Editor.GraphDrawer.InputDrawer
    {
        private readonly Port _port;
        private readonly ISingleInput _content;
        
        public InputDrawer(ISingleInput content, NodeDrawer parent, SerializedProperty inputProperty) 
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
            var connection = _content.Connection;
            if (connection != null)
            {
                if (((UnityReferenceGraphDrawer)Parent.Parent).OutputsMapping.TryGetValue(connection, out var drawer))
                {
                    DrawConnection(drawer);
                }
                else
                {
                    InputProperty.FindPropertyRelative("Connection").managedReferenceValue = null;
                }
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