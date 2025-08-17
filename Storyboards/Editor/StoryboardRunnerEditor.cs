using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace Storyboards.Editor
{
    [CustomEditor(typeof(StoryboardRunner))]
    public class StoryboardRunnerEditor : UnityEditor.Editor
    {
        private SerializedProperty _assetProperty;
        private SerializedProperty _contextProperty;
        
        private SerializedObject _assetSerializedObject;
        private SerializedProperty _blackboardProperty;
        
        private StoryboardRunner _target;
        
        private VisualElement _root;
        
        private VisualElement _assetContainer;
        private VisualElement _helpBox;
        
        private ListView _listView;
        private Button _debugButton;

        public override VisualElement CreateInspectorGUI()
        {
            _target = (StoryboardRunner)target;
            
            _assetProperty = serializedObject.FindProperty("_asset");
            _contextProperty = serializedObject.FindProperty("_context");
            
            _root = new VisualElement();
            _root.Add(new PropertyField(serializedObject.FindProperty("_autoStart")));
            
            CreateAssetPropertyField();
            CreateContextListView();
            
            _debugButton = new Button(OpenDebug) { text = "Runtime debug", };
            
            OnAssetPropertyFieldModified();
            
            return _root;
        }
        
        private void CreateAssetPropertyField()
        {
            _assetContainer = new VisualElement();
            _root.Add(_assetContainer);

            var assetPropertyField = new PropertyField(_assetProperty, "Asset");
            assetPropertyField.Bind(serializedObject);
            assetPropertyField.RegisterValueChangeCallback(OnAssetPropertyModified);
            
            _assetContainer.Add(assetPropertyField);
            _helpBox = new HelpBox(
                "Assign a Storyboard Asset to modify context.",
                HelpBoxMessageType.Info
            );
        }
        
        private void CreateContextListView()
        {
            _listView = new ListView
            {
                makeItem = CreateContextEntryVisualElement,
                bindItem = BindContextEntryVisualElement
            };
            _root.Add(_listView);
        }

        private void OnAssetPropertyModified(SerializedPropertyChangeEvent evt)
        {
            OnAssetPropertyFieldModified();
        }

        private void OnAssetPropertyFieldModified()
        {
            var hasAsset = _target.Asset != null;
            var hasHelpBox = _assetContainer.Contains(_helpBox);
            var hasDebugButton = _assetContainer.Contains(_debugButton);
            
            if (hasAsset)
            {
                _assetContainer.Add(_debugButton);
                if (hasHelpBox)
                {
                    _assetContainer.Remove(_helpBox);
                }
            }
            else
            {
                _assetContainer.Add(_helpBox);
                if (hasDebugButton)
                {
                    _assetContainer.Remove(_debugButton);
                }
            }

            _assetSerializedObject = hasAsset ? new SerializedObject(_target.Asset) : null;
            _blackboardProperty = hasAsset
                ? _assetSerializedObject.FindProperty("<Storyboard>k__BackingField").FindPropertyRelative("_blackboard")
                : null;
            _listView.visible = hasAsset;
            
            _listView.Unbind();
            
            if (!hasAsset)
            {
                return;
            }
            
            _listView.TrackPropertyValue(_blackboardProperty, OnBlackboardPropertyModified);
            SyncContextToBlackboard();
        }
        
        private void OnBlackboardPropertyModified(SerializedProperty property)
        {
            SyncContextToBlackboard();
        }
        
        private void SyncContextToBlackboard()
        {
            var previousContext = new Dictionary<string, IBlackboardVariableContext>();
            foreach (var context in _target.Context)
            {
                if (context == null || string.IsNullOrEmpty(context.Guid))
                {
                    continue;
                }

                if (!previousContext.TryAdd(context.Guid, context))
                {
                    Debug.LogWarning
                    (
                        $"Duplicate context variable ID found: {context.Guid}. " +
                        $"Previously serialized IDs were not unique. Skipping context variable."
                    );
                }
            }

            _contextProperty.ClearArray();
            for (var i = 0; i < _blackboardProperty.arraySize; i++)
            {
                var blackboardVariableProperty = _blackboardProperty.GetArrayElementAtIndex(i);
                var blackboardVariable = blackboardVariableProperty.managedReferenceValue as IBlackboardVariable;

                IBlackboardVariableContext contextVariable = null;
                if (previousContext.TryGetValue(blackboardVariable.Guid, out var existingContext) && 
                    existingContext.GetType() == typeof(BlackboardVariableContext<>).MakeGenericType(blackboardVariable.Type))
                {
                    contextVariable = existingContext;
                }
                else
                {
                    var genericType = typeof(BlackboardVariableContext<>).MakeGenericType(blackboardVariable.Type);
                    contextVariable = Activator.CreateInstance(genericType) as IBlackboardVariableContext;
                    genericType.GetProperty("Guid").SetValue(contextVariable, blackboardVariable.Guid);
                }

                _contextProperty.InsertArrayElementAtIndex(i);
                var contextElementProperty = _contextProperty.GetArrayElementAtIndex(i);
                contextElementProperty.managedReferenceValue = contextVariable;
            }
            serializedObject.ApplyModifiedProperties();
            
            RefreshContextView();
        }
        
        private void RefreshContextView()
        {
            if (_target.Asset == null)
            {
                _listView.itemsSource = new List<SerializedProperty>();
                _listView.RefreshItems();
                return;
            }
            
            _listView.itemsSource = Enumerable.Range(0, _contextProperty.arraySize)
                .Select(i => _contextProperty.GetArrayElementAtIndex(i))
                .ToList();
            _listView.RefreshItems();
        }

        private VisualElement CreateContextEntryVisualElement()
        {
            return new PropertyField();
        }

        private void BindContextEntryVisualElement(VisualElement element, int i)
        {
            var blackboardVariableProperty = _blackboardProperty
                .GetArrayElementAtIndex(i)
                .FindPropertyRelative("<Name>k__BackingField");

            var valueProperty = _contextProperty
                .GetArrayElementAtIndex(i)
                .FindPropertyRelative("<Value>k__BackingField");
            
            var valueField = element as PropertyField;
            if (valueProperty != null)
            {
                valueField.bindingPath = valueProperty.propertyPath;
                valueField.Bind(serializedObject);
                valueField.label = blackboardVariableProperty?.stringValue ?? "Unknown";
            }
        }
        
        private void OpenDebug()
        {
            if (_target.RuntimeAsset == null)
            {
                Debug.LogWarning("Debug is only available in playmode.");
                return;
            }
            
            GraphEditorWindow.OpenGraphWindowForProperty<GraphEditorWindow>(
                _target.RuntimeAsset,
                "<Storyboard>k__BackingField"
            );    
        }
    }
}