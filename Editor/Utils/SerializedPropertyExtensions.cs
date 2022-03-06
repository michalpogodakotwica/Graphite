using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Credit - https://gist.github.com/aholkner/214628a05b15f0bb169660945ac7923b
// Provide simple value get/set methods for SerializedProperty.  Can be used with
// any data types and with arbitrarily deeply-pathed properties.
namespace Graphite.Editor.Utils
{
    public static class SerializedPropertyExtensions
    {
        public static object GetValue(this SerializedProperty property)
        {
            var propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            var i = 0;
            while (NextPathComponent(propertyPath, ref i, out var token))
                value = GetPathComponentValue(value, token);
            return value;
        }
        
        public static void SetValue(this SerializedProperty property, object value)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

            SetValueNoRecord(property, value);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

        public static void SetValueNoRecord(this SerializedProperty property, object value)
        {
            var propertyPath = property.propertyPath;
            object container = property.serializedObject.targetObject;

            var i = 0;
            NextPathComponent(propertyPath, ref i, out var deferredToken);
            while (NextPathComponent(propertyPath, ref i, out var token))
            {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }
            
            Debug.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary. Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
            
            SetPathComponentValue(container, deferredToken, value);
        }

        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            return GetFieldInfo(property.serializedObject.targetObject, property.propertyPath);
        }

        private static FieldInfo GetFieldInfo(object container, string propertyPath)
        {
            var i = 0;
            NextPathComponent(propertyPath, ref i, out var deferredToken);
            while (NextPathComponent(propertyPath, ref i, out var token))
            {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }
            
            var type = container.GetType();
            
            return type.GetField(deferredToken.PropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }
        
        private static void SetPathComponentValue(object container, PropertyPathComponent component, object value)
        {
            if (component.PropertyName == null)
                ((IList)container)[component.ElementIndex] = value;
            else
                SetMemberValue(container, component.PropertyName, value);
        }

        private static void SetMemberValue(object container, string name, object value)
        {
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
            {
                switch (member)
                {
                    case FieldInfo field:
                        field.SetValue(container, value);
                        return;
                    case PropertyInfo property:
                        property.SetValue(container, value);
                        return;
                }
            }
            Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }
        
        private struct PropertyPathComponent
        {
            public string PropertyName;
            public int ElementIndex;
        }

        private static readonly Regex ArrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

        private static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component)
        {
            component = new PropertyPathComponent();

            if (index >= propertyPath.Length)
                return false;

            var arrayElementMatch = ArrayElementRegex.Match(propertyPath, index);
            if (arrayElementMatch.Success)
            {
                index += arrayElementMatch.Length + 1; // Skip past next '.'
                component.ElementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
                return true;
            }

            var dot = propertyPath.IndexOf('.', index);
            if (dot == -1)
            {
                component.PropertyName = propertyPath.Substring(index);
                index = propertyPath.Length;
            }
            else
            {
                component.PropertyName = propertyPath.Substring(index, dot - index);
                index = dot + 1; // Skip past next '.'
            }

            return true;
        }

        private static object GetPathComponentValue(object container, PropertyPathComponent component)
        {
            return component.PropertyName == null ? ((IList)container)[component.ElementIndex] : GetMemberValue(container, component.PropertyName);
        }

        private static object GetMemberValue(object container, string name)
        {
            if (container == null)
                return null;
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var memberInfo in members)
            {
                switch (memberInfo)
                {
                    case FieldInfo field:
                        return field.GetValue(container);
                    case PropertyInfo property:
                        return property.GetValue(container);
                }
            }
            return null;
        }
    }
}