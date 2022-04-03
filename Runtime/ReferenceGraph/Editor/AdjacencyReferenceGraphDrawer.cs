using com.michalpogodakotwica.graphite.Editor;
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;

namespace ReferenceGraph.Editor
{
    [CustomGraphDrawer(typeof(Graph))]
    public class AdjacencyReferenceGraphDrawer : GraphDrawer
    {
        public AdjacencyReferenceGraphDrawer(GraphEditorWindow editorWindow) : base(editorWindow)
        {
            GraphSerializationBackend = new GraphSerialization();
        }
    }
}