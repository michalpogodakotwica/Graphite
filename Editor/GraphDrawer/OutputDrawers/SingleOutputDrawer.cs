using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Runtime;
using Graphite.Runtime.Ports;
using UnityEditor;

namespace Graphite.Editor.GraphDrawer.OutputDrawers
{
    [CustomOutputDrawer(typeof(IOutput))]
    public class SingleOutputDrawer : OutputDrawer
    {
        public SingleOutputDrawer(Output content, NodeDrawer parent, SerializedProperty outputProperty) 
            : base(content, parent, outputProperty)
        {
        }
        
        public override void DrawPort()
        {
            Parent.AddOutputPort(Port, this);
            PortContainer.Add(Port);
        }
        
        public override void ClearPort()
        {
            Parent.RemoveOutputPort(Port);
            PortContainer.Remove(Port);
        }
    }
}