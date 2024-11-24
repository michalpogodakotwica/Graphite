using System.Collections.Generic;
using Graphite.Editor;
using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;

namespace Graphite.UnityReferenceGraph.Editor
{
    [CustomGraphDrawer(typeof(Graph))]
    public class UnityReferenceGraphDrawer : GraphDrawer
    {
        public readonly Dictionary<IOutput, OutputDrawer> OutputsMapping = new();
        
        public UnityReferenceGraphDrawer(GraphEditorWindow editorWindow) : base(editorWindow)
        {
            GraphSerializationBackend = new GraphSerialization();
        }
    }
}