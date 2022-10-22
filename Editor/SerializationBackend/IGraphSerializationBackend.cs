using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace com.michalpogodakotwica.graphite.Editor.SerializationBackend
{
    public interface IGraphSerializationBackend
    {
        IEnumerable<(SerializedProperty, INode)> GetAllNodes(GraphDrawer.GraphDrawer graphDrawer);
        IEnumerable<(SerializedProperty, INode)> AddNodes(GraphDrawer.GraphDrawer graphDrawer, List<INode> nodesToAdd);
        GraphViewChange SerializeGraphViewChange(GraphDrawer.GraphDrawer graphDrawer, GraphViewChange viewChanges);
    }
}