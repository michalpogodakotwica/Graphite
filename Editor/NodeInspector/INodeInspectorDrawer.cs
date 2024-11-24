using Graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEngine.UIElements;

namespace Graphite.Editor.NodeInspector
{
    public interface INodeInspector
    {
        VisualElement CreateInspectorGUI(NodeDrawer drawer);
        
        void Dispose();
    }
}