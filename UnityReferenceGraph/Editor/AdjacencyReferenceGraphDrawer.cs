using System.Collections.Generic;
using com.michalpogodakotwica.graphite.Editor;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Editor
{
    //[CustomGraphDrawer(typeof(Graph))]
    public class AdjacencyReferenceGraphDrawer : GraphDrawer
    {
        public readonly Dictionary<IOutput, OutputDrawer> OutputsMapping = new();
        
        public AdjacencyReferenceGraphDrawer(GraphEditorWindow editorWindow) : base(editorWindow)
        {
            GraphSerializationBackend = new GraphSerialization();
        }
    }
}