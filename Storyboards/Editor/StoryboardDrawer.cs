using System;
using System.Collections.Generic;
using Graphite.Editor;
using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;
using Graphite.Editor.Utils;
using Graphite.UnityReferenceGraph.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Storyboards.Editor
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [CustomGraphDrawer(typeof(Storyboard))]
    public class StoryboardDrawer : UnityReferenceGraphDrawer
    {
        private readonly Blackboard _blackboard;
        private readonly BlackboardSection _blackboardSection;
        
        private readonly CreateBlackboardVariableSearchWindowProvider _createBlackboardVariableSearchWindowProvider;
        
        public StoryboardDrawer(GraphEditorWindow editorWindow) : base(editorWindow)
        {
            _createBlackboardVariableSearchWindowProvider = ScriptableObject.CreateInstance<CreateBlackboardVariableSearchWindowProvider>();
            _createBlackboardVariableSearchWindowProvider.Initialize(this);
            
            _blackboard = new Blackboard
            {
                graphView = this,
                scrollable = true
            };
            
            // bullshit for reordering
            _blackboardSection = new BlackboardSection
            {
                title = "Variables",
                name = "VariablesSection"
            };
            _blackboard.Add(_blackboardSection);
            
            _blackboard.addItemRequested += OpenVariableCreationWindow;
            _blackboard.editTextRequested += RenameVariable;
            _blackboard.moveItemRequested += MoveVariable;
            graphViewChanged += OnGraphViewChanged;
            
            AddBlackboardContextMenuManipulators(_blackboard);
        }

        public override void Dispose()
        {
            _blackboard.addItemRequested -= OpenVariableCreationWindow;
            _blackboard.editTextRequested -= RenameVariable;
            _blackboard.moveItemRequested -= MoveVariable;
            graphViewChanged -= OnGraphViewChanged;
            
            base.Dispose();
        }

        protected override void DrawGraph()
        {
            DrawBlackboard();
            base.DrawGraph();
        }
        
        protected override void ClearGraph()
        {
            ClearBlackboard();
            base.ClearGraph();
        }
        
        private void DrawBlackboard()
        {
            foreach (var blackboardVariable in GetBlackboard(this))
            {
                AddVariableView(blackboardVariable);
            }
        }
        
        public IEnumerable<IBlackboardVariable> GetBlackboard(GraphDrawer graphDrawer)
        {
            var _blackboard = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            _blackboard.serializedObject.Update();

            for (var i = 0; i < _blackboard.arraySize; i++)
            {
                var blackboardVariableProperty = _blackboard.GetArrayElementAtIndex(i);
                var blackboardVariable = (IBlackboardVariable)blackboardVariableProperty.GetValue();
                yield return blackboardVariable;
            }
        }
        
        public IEnumerable<SerializedProperty> GetBlackboardProperties(Type type, GraphDrawer graphDrawer)
        {
            var blackboardProperty = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            blackboardProperty.serializedObject.Update();

            for (var i = 0; i < blackboardProperty.arraySize; i++)
            {
                var blackboardVariableProperty = blackboardProperty.GetArrayElementAtIndex(i);
                var blackboardVariable = (IBlackboardVariable)blackboardVariableProperty.GetValue();
                var blackboardVariableType = blackboardVariable.Type;
                
                if (type != blackboardVariableType)
                {
                    continue;
                }
                
                if (blackboardVariable != null)
                {
                    yield return blackboardVariableProperty;
                }
            }
        }
        
        public IEnumerable<IBlackboardVariable> GetBlackboardVariables(Type type, GraphDrawer graphDrawer)
        {
            var blackboardProperty = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            blackboardProperty.serializedObject.Update();

            for (var i = 0; i < blackboardProperty.arraySize; i++)
            {
                var blackboardVariableProperty = blackboardProperty.GetArrayElementAtIndex(i);
                var blackboardVariable = (IBlackboardVariable)blackboardVariableProperty.GetValue();
                var blackboardVariableType = blackboardVariable.Type;
                
                if (type != blackboardVariableType)
                {
                    continue;
                }
                
                if (blackboardVariable != null)
                {
                    yield return blackboardVariable;
                }
            }
        }
        
        private void ModifyBlackboardWithUndo(GraphDrawer graphDrawer, Action applyModification)
        {
            var blackboardProperty = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            blackboardProperty.serializedObject.Update();
            Undo.RecordObject(blackboardProperty.serializedObject.targetObject, $"Blackboard changes");
            applyModification();
            EditorUtility.SetDirty(blackboardProperty.serializedObject.targetObject);
            blackboardProperty.serializedObject.ApplyModifiedProperties();
        }

        private void AddVariableWithoutUndo(GraphDrawer graphDrawer, IBlackboardVariable variable)
        {
            var blackboard = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            var size = blackboard.arraySize;
            blackboard.arraySize++;
            blackboard.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            
            blackboard.GetArrayElementAtIndex(size).managedReferenceValue = variable;
            blackboard.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void RemoveVariableWithoutUndo(GraphDrawer graphDrawer, IBlackboardVariable variable)
        {
            var indexToRemove = -1;
            var blackboard = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
         
            for (var i = 0; i < blackboard.arraySize; i++)
            {
                var blackboardVariableProperty = blackboard.GetArrayElementAtIndex(i);
                var blackboardVariable = (IBlackboardVariable)blackboardVariableProperty.GetValue();
                if (blackboardVariable == variable)
                {
                    indexToRemove = i;
                    break;
                }
            }
            
            if (indexToRemove < 0)
            {
                return;
            }
            
            blackboard.GetArrayElementAtIndex(indexToRemove).SetValue(null);
            blackboard.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            blackboard.DeleteArrayElementAtIndex(indexToRemove);
            blackboard.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private void MoveVariableWithoutUndo(GraphDrawer graphDrawer, IBlackboardVariable variable, int newIndex)
        {
            var blackboard = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            var indexToMove = -1;

            for (var i = 0; i < blackboard.arraySize; i++)
            {
                var blackboardVariableProperty = blackboard.GetArrayElementAtIndex(i);
                var blackboardVariable = (IBlackboardVariable)blackboardVariableProperty.GetValue();
                if (blackboardVariable == variable)
                {
                    indexToMove = i;
                    break;
                }
            }

            if (indexToMove < 0 || newIndex < 0 || newIndex >= blackboard.arraySize)
            {
                return;
            }

            blackboard.MoveArrayElement(indexToMove, newIndex);
            blackboard.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private void RenameVariableWithoutUndo(GraphDrawer graphDrawer, IBlackboardVariable variable, string newText)
        {
            var blackboard = graphDrawer.GraphProperty.FindPropertyRelative("_blackboard");
            for (var i = 0; i < blackboard.arraySize; i++)
            {
                var blackboardVariableProperty = blackboard.GetArrayElementAtIndex(i);
                var blackboardVariable = (IBlackboardVariable)blackboardVariableProperty.GetValue();
                
                if (blackboardVariable == variable)
                {
                    var nameProperty = blackboardVariableProperty.FindPropertyRelative("<Name>k__BackingField");
                    nameProperty.stringValue = newText;
                    blackboardVariableProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private void ClearBlackboard()
        {
            _blackboardSection.Clear();
        }
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is BlackboardField blackboardField)
                    {
                        RemoveVariable(blackboardField);
                    }
                }
            }
            return graphViewChange;
        }

        private void OpenVariableCreationWindow(Blackboard context)
        {
            var searchWindowContext = new SearchWindowContext(MouseScreenPosition);
            SearchWindow.Open(searchWindowContext, _createBlackboardVariableSearchWindowProvider);
        }
        
        public void AddVariable(Type variableType)
        {
            var genericType = typeof(BlackboardVariable<>);
            var constructedType = genericType.MakeGenericType(variableType);
            var variable = Activator.CreateInstance(constructedType) as IBlackboardVariable;
            AddVariableView(variable);
            ModifyBlackboardWithUndo(this, () => AddVariableWithoutUndo(this, variable));
        }

        private void AddVariableView(IBlackboardVariable variable)
        {
            var blackboardField = new BlackboardField
            {
                text = variable.Name,
                typeText = variable.Type.Name,
                userData = variable
            };
            
            blackboardField.AddManipulator(new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction
                (
                    "Remove Variable",
                    (a) => { RemoveVariable(blackboardField); }, 
                    a => DropdownMenuAction.Status.Normal
                );
            }));
            
            _blackboardSection.Add(blackboardField);
        }

        public void RemoveVariable(BlackboardField blackboardField)
        {
            _blackboardSection.Remove(blackboardField);
            ModifyBlackboardWithUndo(this,
                () => RemoveVariableWithoutUndo(this, (IBlackboardVariable)blackboardField.userData));
        }
        
        public void MoveVariable(Blackboard blackboard, int newIndex, VisualElement element)
        {
            _blackboardSection.Insert(newIndex, element);
            var variable = (IBlackboardVariable)((BlackboardField)element).userData;
            ModifyBlackboardWithUndo(this, () => MoveVariableWithoutUndo(this, variable, newIndex));
        }
        
        public void RenameVariable(Blackboard blackboard, VisualElement visualElement, string newText)
        {
            if (visualElement is BlackboardField blackboardField)
            {
                blackboardField.text = newText;
            }
            
            ModifyBlackboardWithUndo(this, () => RenameVariableWithoutUndo(this, (IBlackboardVariable)visualElement.userData, newText));
        }
        
        private void AddBlackboardContextMenuManipulators(Blackboard blackboard)
        {
            blackboard.AddManipulator(new ContextualMenuManipulator((evt) =>
            {
                evt.menu.AppendAction
                (
                    "Hide Blackboard",
                    (a) => { Remove(blackboard); }, 
                    a => DropdownMenuAction.Status.Normal
                );
            }));
            
            var showBlackboardManipulator = new ContextualMenuManipulator((evt) =>
            {
                if (blackboard.parent == null)
                {
                    evt.menu.AppendAction(
                        "Show Blackboard", 
                        (a) => { Add(blackboard); }, 
                        a => DropdownMenuAction.Status.Normal
                    );
                }
            });
            this.AddManipulator(showBlackboardManipulator);
        }
    }
}