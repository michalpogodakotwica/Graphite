using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Graphite.Editor.CompatiblePortsProvider
{
    public interface ICompatiblePortsProvider
    {
        List<Port> GetCompatiblePorts(GraphView graphView, Port startPort,
            NodeAdapter nodeAdapter);
    }
}