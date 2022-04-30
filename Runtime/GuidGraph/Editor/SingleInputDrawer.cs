using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.GuidGraph.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace com.michalpogodakotwica.graphite.GuidGraph.Editor
{
    [CustomInputDrawer(typeof(ISingleInput))]
    public class InputDrawer : com.michalpogodakotwica.graphite.Editor.GraphDrawer.InputDrawer
    {
        private readonly Port _port;
        private readonly ISingleInput _content;
        
        public InputDrawer(Input content, NodeDrawer parent, SerializedProperty inputProperty) 
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
            var connection = _content?.Connection;
            if (connection != null &&
                ((GuidGraphDrawer)Parent.Parent).OutputsMapping.TryGetValue(connection.Value, out var drawer))
            {
                DrawConnection(drawer);
            }
        }

        private void DrawConnection(OutputDrawer outputDrawer)
        {
            var edge = _port.ConnectTo<Edge>(outputDrawer.Port);
            Parent.Parent.AddElement(edge);
        }

        public override bool TryToConnect(OutputDrawer outputDrawer, Edge edge)
        {
            InputProperty.FindPropertyRelative("_guid").stringValue = ((Output)outputDrawer.Content).Guid.ToString();
            ((Input)Content).Connect((Output)outputDrawer.Content);
            return true;
        }
        
        public override bool TryToDisconnect(OutputDrawer outputDrawer, Edge edge)
        {
            InputProperty.FindPropertyRelative("_guid").stringValue = null;
            ((Input)Content).Disconnect((Output)outputDrawer.Content);
            return true;
        }
    }
}