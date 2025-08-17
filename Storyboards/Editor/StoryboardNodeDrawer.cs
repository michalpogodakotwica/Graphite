using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Storyboards.Editor
{
    [CustomNodeDrawer(typeof(Node))]
    public class StoryboardNodeDrawer : InspectorNodeDrawer
    {
        private readonly List<string> _runtimeStyles = new();
        
        public StoryboardNodeDrawer(Graphite.INode content, GraphDrawer parent,
            SerializedProperty contentSerializedProperty,
            NodeViewSettings nodeViewSettings) : base(content, parent,
            contentSerializedProperty, nodeViewSettings)
        {
        }

        public override void AddNode()
        {
            base.AddNode();

            if (Content is IRuntimeStylable runtimeStylable)
            {
                UpdateRuntimeStyles(runtimeStylable.Styles);
                runtimeStylable.UpdateStyles += UpdateRuntimeStyles;
            }
        }

        public override void ClearNode()
        {
            base.ClearNode();

            if (Content is IRuntimeStylable runtimeStylable)
            {
                runtimeStylable.UpdateStyles -= UpdateRuntimeStyles;
            }
        }

        private void UpdateRuntimeStyles(IEnumerable<string> styles)
        {
            foreach (var runtimeStyle in _runtimeStyles)
            {
                RemoveFromClassList(runtimeStyle);
            }
            _runtimeStyles.Clear();
            _runtimeStyles.AddRange(styles);
            foreach (var runtimeStyle in _runtimeStyles)
            {
                AddToClassList(runtimeStyle);
            }
        }

        protected override IBindable CreateControlField(FieldInfo field)
        {
            return typeof(IBlackboardVariable).IsAssignableFrom(field.FieldType) 
                ? CreateBlackboardVariableControlField(field) 
                : base.CreateControlField(field);
        }

        private IBindable CreateBlackboardVariableControlField(FieldInfo field)
        {
            ContentSerializedProperty.serializedObject.Update();
            
            var graphDrawer = (StoryboardDrawer)Parent;
            var variableType = field.FieldType.GetGenericArguments()[0];
            var variables = graphDrawer.GetBlackboardVariables(variableType, graphDrawer);
            var popupField = new PopupField<IBlackboardVariable>(
                new IBlackboardVariable[] { null }.Concat(variables).ToList(),
                (IBlackboardVariable)field.GetValue(Content),
                formatListItemCallback: v => v?.Name ?? "None", 
                formatSelectedValueCallback: v => v?.Name ?? "None"
            )
            {
                label = ObjectNames.NicifyVariableName(field.Name)
            };
            
            popupField.RegisterValueChangedCallback(e => SetBlackboardVariableValue(e, field.Name));
            OnBlackboardChanged(field, popupField);
            return popupField;
        }

        private void SetBlackboardVariableValue(ChangeEvent<IBlackboardVariable> evt, string fieldName)
        {
            var property = ContentSerializedProperty.FindPropertyRelative(fieldName);
            property.managedReferenceValue = evt.newValue;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnBlackboardChanged(FieldInfo fieldInfo, PopupField<IBlackboardVariable> popupField)
        {
            var graphDrawer = (StoryboardDrawer)Parent;
            var variableType = fieldInfo.FieldType.GetGenericArguments()[0];
            var properties = graphDrawer.GetBlackboardProperties(variableType, graphDrawer);
            
            popupField.Unbind();
            foreach (var property in properties)
            {
                popupField.TrackPropertyValue(property.FindPropertyRelative("<Name>k__BackingField"), _ => UpdateNames(fieldInfo, popupField));
            }
            popupField.TrackPropertyValue(graphDrawer.GraphProperty.FindPropertyRelative("_blackboard"), _ => OnBlackboardChanged(fieldInfo, popupField));
            
            var variables = graphDrawer.GetBlackboardVariables(variableType, graphDrawer);
            var choices = new IBlackboardVariable[] { null }.Concat(variables).ToList();
            popupField.choices = choices;
        }

        private void UpdateNames(FieldInfo fieldInfo, PopupField<IBlackboardVariable> popupField)
        {
            var graphDrawer = (StoryboardDrawer)Parent;
            var variableType = fieldInfo.FieldType.GetGenericArguments()[0];
            var variables = graphDrawer.GetBlackboardVariables(variableType, graphDrawer);
            var choices = new IBlackboardVariable[] { null }.Concat(variables).ToList();

            if (choices.Contains(popupField.value))
            {
                popupField.SetValueWithoutNotify(popupField.value);
                return;
            }

            var property = ContentSerializedProperty.FindPropertyRelative(fieldInfo.Name);
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
            popupField.value = null;
        }
    }
}