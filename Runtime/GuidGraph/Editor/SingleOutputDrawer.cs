using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.GuidGraph.Runtime;
using UnityEditor;

namespace com.michalpogodakotwica.graphite.GuidGraph.Editor
{
    [CustomOutputDrawer(typeof(Output))]
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

        public override void AddPort()
        {
            ((GuidGraphDrawer)Parent.Parent).OutputsMapping.Add(((Output)Content).Guid, this);
        }

        public override void RemovePort()
        {
            ((GuidGraphDrawer)Parent.Parent).OutputsMapping.Remove( ((Output)Content).Guid);
        }
    }
}