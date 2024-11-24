using System;
using System.Collections.Generic;
using Graphite.Editor;
using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;
using Graphite.GuidGraph.Runtime;

namespace Graphite.GuidGraph.Editor
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