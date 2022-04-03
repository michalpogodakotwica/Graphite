﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.michalpogodakotwica.graphite.Attributes;
using com.michalpogodakotwica.graphite.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers
{
	[CustomNodeDrawer(typeof(INode))]
	public class InspectorNodeDrawer : NodeDrawer
	{
		private readonly Dictionary<FieldInfo, PropertyField> _fields;

		public InspectorNodeDrawer(INode content, GraphDrawer parent, SerializedProperty contentSerializedProperty,
			NodeViewSettings nodeViewSettings) : base(content, parent, contentSerializedProperty, nodeViewSettings)
		{
			_fields = Content.GetType()
				.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(f => !typeof(IInput).IsAssignableFrom(f.FieldType) && !typeof(IOutput).IsAssignableFrom(f.FieldType))
				.Where(f => f.GetCustomAttributes().Any(a => a is ShowInNodeAttribute))
				.ToDictionary(k => k, CreateControlField);
		}

		public override void ReassignProperty(SerializedProperty contentSerializedProperty)
		{
			base.ReassignProperty(contentSerializedProperty);
			foreach (var (key, value) in _fields)
			{
				value.Unbind();
				value.BindProperty(contentSerializedProperty.FindPropertyRelative(key.Name).serializedObject);
			}
		}

		public override void ClearNode()
		{
			base.ClearNode();
			foreach (var controlField in _fields.Values)
			{
				controlField.Unbind();
			}
		}

		public override void AddNode()
		{
			base.AddNode();
			RefreshPorts();
			DrawContent();
		}

		private void DrawContent()
		{
			foreach (var controlField in _fields.Values)
			{
				mainContainer.Add(controlField);
				controlField.BindProperty(ContentSerializedProperty.serializedObject);
			}
			
			RefreshExpandedState();
		}

		private PropertyField CreateControlField(FieldInfo field)
		{
			if (field == null)
				return null;

			var showInNodeAttribute = field.GetCustomAttribute<ShowInNodeAttribute>();
			
			var label = showInNodeAttribute.HideLabel 
				? "" 
				: showInNodeAttribute.LabelText ?? ObjectNames.NicifyVariableName(field.Name);

			ContentSerializedProperty.serializedObject.Update();
			
			var element = new PropertyField(ContentSerializedProperty.FindPropertyRelative(field.Name), label);
			element.SetEnabled(showInNodeAttribute.IsReadOnly);
			return element;
		}
	}
}