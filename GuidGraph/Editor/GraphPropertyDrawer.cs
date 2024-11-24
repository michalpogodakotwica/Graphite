using Graphite.Editor;
using Graphite.GuidGraph.Runtime;
using UnityEditor;

namespace Graphite.GuidGraph.Editor
{
    [CustomPropertyDrawer(typeof(Graph), true)]
    public class GraphPropertyDrawer : Graphite.Editor.GraphPropertyDrawer
    {
        protected override void OpenGraphProperty(SerializedProperty property)
        {
            GraphEditorWindow.OpenGraphWindowForProperty<GuidGraphEditor>(
                property.serializedObject.targetObject,
                property.propertyPath
            );
        }
    }
}