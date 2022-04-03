using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.Editor.NodeInspector
{
    public interface INodeInspector
    {
        VisualElement CreateInspectorGUI(NodeDrawer drawer);
        
        void Dispose();
    }
}