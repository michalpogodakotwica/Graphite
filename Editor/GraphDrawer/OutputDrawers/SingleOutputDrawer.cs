using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEditor;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer.OutputDrawers
{
    [CustomOutputDrawer(typeof(IOutput))]
    public class SingleOutputDrawer : OutputDrawer
    {
        public SingleOutputDrawer(IOutput content, NodeDrawer parent, SerializedProperty outputProperty) 
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