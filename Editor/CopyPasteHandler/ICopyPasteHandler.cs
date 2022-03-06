using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Graphite.Editor.CopyPasteHandler
{
    public interface ICopyPasteHandler
    {
	    string SerializeGraphElementsCallback(GraphDrawer.GraphDrawer graphDrawer, IEnumerable<GraphElement> elements);
	    bool CanPasteSerializedDataCallback(GraphDrawer.GraphDrawer graphDrawer, string data);
	    public void UnserializeAndPasteCallback(GraphDrawer.GraphDrawer graphDrawer, string operationName, string data);
    }
}