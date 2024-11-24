using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.GuidGraph.Runtime;
using UnityEditor;

namespace Graphite.GuidGraph.Editor
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