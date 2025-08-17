using Graphite.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Storyboards.Editor
{
    public static class StoryboardAssetEditor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var target = EditorUtility.InstanceIDToObject(instanceID) as StoryboardAsset;
            
            if (target == null)
                return false;
            
            GraphEditorWindow.OpenGraphWindowForProperty<GraphEditorWindow>(
                target,
                "<Storyboard>k__BackingField"
            );
            return true;
        }
    }
}