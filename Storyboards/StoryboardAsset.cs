using Graphite.Attributes;
using UnityEngine;

namespace Storyboards
{
    [CreateAssetMenu(fileName = "StoryboardAsset", menuName = "Storyboards/StoryboardAsset", order = 0)]
    public class StoryboardAsset : ScriptableObject
    {
        [field: SerializeField, GraphViewSettings(graphStyleSheetsResourcePaths: "Storyboard")]
        public Storyboard Storyboard { get; private set; } = new();
    }
}