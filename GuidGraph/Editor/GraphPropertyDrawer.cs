using com.michalpogodakotwica.graphite.Editor;
using com.michalpogodakotwica.graphite.GuidGraph.Runtime;
using UnityEditor;

namespace com.michalpogodakotwica.graphite.GuidGraph.Editor
{
    [CustomPropertyDrawer(typeof(Graph), true)]
    public class GraphPropertyDrawer : graphite.Editor.GraphPropertyDrawer
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