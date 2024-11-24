using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphite.Editor.GraphDrawer
{
    public static class Minimap
    {
        public static void AddMinimapManipulator(VisualElement parent)
        {
            var minimap = new MiniMap {anchored = false};
            minimap.SetPosition(new Rect(10, 30, 200, 140));
            minimap.AddManipulator(new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction
                (
                    "Hide Minimap",
                    (a) => { parent.Remove(minimap); }, 
                    a => DropdownMenuAction.Status.Normal
                );
            }));
            
            var minimapManipulator = new ContextualMenuManipulator((evt) =>
            {
                if (minimap.parent == null)
                {
                    evt.menu.AppendAction(
                        "Add Minimap", 
                        (a) => { parent.Add(minimap); }, 
                        a => DropdownMenuAction.Status.Normal
                    );
                }
            });
            
            parent.AddManipulator(minimapManipulator);
        }
    }
}