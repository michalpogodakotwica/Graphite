using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.Editor.NodeInspector
{
    [CustomNodeInspectorDrawer(typeof(INode))]
    public class DefaultNodeInspector : INodeInspector
    {
        private Dictionary<FieldInfo, PropertyField> _fields;
        private VisualElement _container;
        private NodeDrawer _drawer;
        private SerializedProperty _contentSerializedProperty;

        public VisualElement CreateInspectorGUI(NodeDrawer drawer)
        {
            _container = new VisualElement();

            _drawer = drawer;
            _contentSerializedProperty = drawer.ContentSerializedProperty;
		    
            drawer.OnSerializedPropertyReassigned += ReassignContentProperty;

            _fields = drawer.Content.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => !typeof(IInput).IsAssignableFrom(f.FieldType) && !typeof(IOutput).IsAssignableFrom(f.FieldType) && f.GetCustomAttributes().Any(a => a is SerializeField))
                .Where(f => !f.GetCustomAttributes().Any(a => a is HideInInspector))
                .ToDictionary(k => k, CreateControlField);
		    
            foreach (var (key, value) in _fields)
            {
                _container.Add(value);
                value.BindProperty(_contentSerializedProperty.FindPropertyRelative(key.Name).serializedObject);
            }

            return _container;
        }

        private PropertyField CreateControlField(FieldInfo field)
        {
            if (field == null)
                return null;

            var label = ObjectNames.NicifyVariableName(field.Name);

            var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
            if (inspectorNameAttribute != null)
                label = inspectorNameAttribute.displayName;

            _contentSerializedProperty.serializedObject.Update();
			
            var element = new PropertyField(_contentSerializedProperty.FindPropertyRelative(field.Name), label);
			
            return element;
        }
	    
        private void ReassignContentProperty(SerializedProperty contentSerializedProperty)
        {
            _contentSerializedProperty = contentSerializedProperty;
            foreach (var (key, value) in _fields)
            {
                value.Unbind();
                value.BindProperty(contentSerializedProperty.FindPropertyRelative(key.Name).serializedObject);
            }
        }
        
        public void Dispose()
        {
            _drawer.OnSerializedPropertyReassigned -= ReassignContentProperty;
            foreach (var controlField in _fields.Values)
            {
                _container.Remove(controlField);
                controlField.Unbind();
            }
        }
    }
}