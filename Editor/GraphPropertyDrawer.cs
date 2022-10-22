using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.Editor
{
    /// Draws button next to graph property that opens GraphEditorWindow.
    [CustomPropertyDrawer(typeof(IGraph), true)]
    public class GraphPropertyDrawer : PropertyDrawer 
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var openGraphEditorButton = new Button(() => OpenGraphProperty(property))
            {
                text = "Open graph editor"
            };

            container.Add(openGraphEditorButton);
            return container;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (GUI.Button(position, "Open graph editor"))
                OpenGraphProperty(property);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        protected virtual void OpenGraphProperty(SerializedProperty property)
        {
            GraphEditorWindow.OpenGraphWindowForProperty<GraphEditorWindow>(
                property.serializedObject.targetObject,
                property.propertyPath
            );
        }
    }
}