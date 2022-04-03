using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

namespace com.michalpogodakotwica.graphite.Editor.CompatiblePortsProvider
{
    public class DefaultPortsProvider : ICompatiblePortsProvider
    {
        public List<Port> GetCompatiblePorts(GraphView graphView, Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            graphView.ports.ForEach(endPort =>
            {
                if (endPort.connections.Any(c => c.input == startPort || c.output == startPort))
                    return;
                
                if (startPort.direction == endPort.direction)
                    return;
                    
                if (!startPort.portType.IsAssignableFrom(endPort.portType))
                    return;
                
                if (startPort != endPort)
                    compatiblePorts.Add(endPort);
            });
            return compatiblePorts;
        }
    }
}