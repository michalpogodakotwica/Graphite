using System.Collections.Generic;
using com.michalpogodakotwica.graphite.Editor;
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Editor
{
    [CustomGraphDrawer(typeof(Graph))]
    public class AdjacencyReferenceGraphDrawer : GraphDrawer
    {
        public readonly Dictionary<IOutput, OutputDrawer> OutputsMapping = new();
        
        public AdjacencyReferenceGraphDrawer(GraphEditorWindow editorWindow) : base(editorWindow)
        {
            GraphSerializationBackend = new GraphSerialization();
        }
    }
}