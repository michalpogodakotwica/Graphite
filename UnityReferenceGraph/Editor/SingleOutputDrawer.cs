using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEditor;

namespace Graphite.UnityReferenceGraph.Editor
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
        
        
        public override void AddPort()
        {
            ((UnityReferenceGraphDrawer)Parent.Parent).OutputsMapping.Add(Content, this);
        }

        public override void RemovePort()
        {
            ((UnityReferenceGraphDrawer)Parent.Parent).OutputsMapping.Remove(Content);
        }
    }
}