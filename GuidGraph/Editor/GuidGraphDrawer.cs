using System;
using System.Collections.Generic;
using com.michalpogodakotwica.graphite.Editor;
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer;
using com.michalpogodakotwica.graphite.GuidGraph.Runtime;

namespace com.michalpogodakotwica.graphite.GuidGraph.Editor
{
    [Serializable]
    [CustomGraphDrawer(typeof(Graph))]
    public class GuidGraphDrawer : GraphDrawer
    {
        public readonly Dictionary<Guid, OutputDrawer> OutputsMapping = new();

        public GuidGraphDrawer(GraphEditorWindow editorWindow) : base(editorWindow)
        {
            GraphSerializationBackend = new GraphSerialization();
        }
    }
}