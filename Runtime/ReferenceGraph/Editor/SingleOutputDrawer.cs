using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEditor;

namespace com.michalpogodakotwica.graphite.ReferenceGraph.Editor
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
            ((AdjacencyReferenceGraphDrawer)Parent.Parent).OutputsMapping.Add(Content, this);
        }

        public override void RemovePort()
        {
            ((AdjacencyReferenceGraphDrawer)Parent.Parent).OutputsMapping.Remove(Content);
        }
    }
}