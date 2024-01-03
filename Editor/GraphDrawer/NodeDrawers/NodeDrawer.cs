using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.michalpogodakotwica.graphite.Editor.Settings;
using com.michalpogodakotwica.graphite.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers
{
    public abstract class NodeDrawer : Node
    {
        public NodeViewSettings NodeViewSettings { get; }
        public GraphViewSettings GraphViewSettings => Parent.Settings;
        
        public readonly GraphDrawer Parent;
        public readonly INode Content;

        public event Action<SerializedProperty> OnSerializedPropertyReassigned;

        protected readonly List<(FieldInfo, InputDrawer)> InputDrawers = new();
        protected readonly List<(FieldInfo, OutputDrawer)> OutputDrawers = new();

        protected readonly Dictionary<Port, InputDrawer> inputMap = new();
        protected readonly Dictionary<Port, OutputDrawer> outputMap = new();
        
        public SerializedProperty ContentSerializedProperty { get; private set; }
        
        protected NodeDrawer(INode content, GraphDrawer parent, SerializedProperty contentSerializedProperty, NodeViewSettings nodeViewSettings)
        {
            Content = content;
            Parent = parent;
            NodeViewSettings = nodeViewSettings;
            ContentSerializedProperty = contentSerializedProperty;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            NodeInspector.Selection.SelectNode(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            NodeInspector.Selection.DeselectNode(this);
        }

        public virtual void ReassignProperty(SerializedProperty contentSerializedProperty)
        {
            ContentSerializedProperty = contentSerializedProperty;
            foreach (var input in InputDrawers)
            {
                input.Item2.ReassignProperty(ContentSerializedProperty.FindPropertyRelative(input.Item1.Name));
            }
            foreach (var output in OutputDrawers)
            {
                output.Item2.ReassignProperty(ContentSerializedProperty.FindPropertyRelative(output.Item1.Name));
            }
            OnSerializedPropertyReassigned?.Invoke(contentSerializedProperty);
        }

        public virtual void AddNode()
        {
            title = NodeViewSettings.NodeTitle;

            LoadPosition();
            
            DrawInputs();
            DrawOutputs();
        }

        public virtual void ClearNode()
        {
            ClearInputs();
            ClearOutputs();
        }
        
        public virtual bool TryToMove(Vector2 movementDelta)
        {
            Content.Position += movementDelta;
            return true;
        }

        public virtual void DrawConnections()
        {
            foreach (var inputView in InputDrawers)
            {
                inputView.Item2.DrawConnections();
            }
        }
        
        protected virtual void LoadPosition()
        {
            var previousPosition = GetPosition();
            var position = Content.Position;
            SetPosition(new Rect(
                position.x, 
                position.y, 
                previousPosition.width,
                previousPosition.height)
            );
        }

        protected virtual void DrawInputs()
        {
            var inputFields = GetFieldsOfType(typeof(IInput));
            foreach (var inputField in inputFields)
            {
                var inputContent = (IInput) inputField.GetValue(Content);
                var inputDrawerType = GraphViewSettings.InputDrawerTypeMapping.GetDrawerForType(inputContent.GetType());
                var drawer = (InputDrawer) Activator.CreateInstance(inputDrawerType,
                    inputContent,
                    this,
                    ContentSerializedProperty.FindPropertyRelative(inputField.Name)
                );

                InputDrawers.Add((inputField, drawer));
                drawer.DrawPorts();
            }
        }

        public void AddInputPort(Port port, InputDrawer owner)
        {
            inputMap[port] = owner;
        }
        
        public void RemoveInputPort(Port port)
        {
            inputMap.Remove(port);
        }
        
        public void AddOutputPort(Port port, OutputDrawer owner)
        {
            outputMap[port] = owner;
        }
        
        public void RemoveOutputPort(Port port)
        {
            outputMap.Remove(port);
        }
        
        protected virtual void DrawOutputs()
        {
            var outputFields = GetFieldsOfType(typeof(IOutput));
            foreach (var outputField in outputFields)
            {
                var outputContent = (IOutput) outputField.GetValue(Content);
                var outputDrawerType = GraphViewSettings.OutputDrawerTypeMapping.GetDrawerForType(outputContent.GetType());
                var drawer = (OutputDrawer) Activator.CreateInstance(
                    outputDrawerType,
                    outputContent,
                    this,
                    ContentSerializedProperty.FindPropertyRelative(outputField.Name)
                );
                
                drawer.AddPort();
                OutputDrawers.Add((outputField, drawer));
                drawer.DrawPort();
            }
        }

        protected virtual void ClearInputs()
        {
            foreach (var inputView in InputDrawers)
            {
                inputView.Item2.ClearPorts();
            }
            InputDrawers.Clear();
        }
        
        protected virtual void ClearOutputs()
        {
            foreach (var outputView in OutputDrawers)
            {
                outputView.Item2.ClearPort();
                outputView.Item2.RemovePort();
            }
            OutputDrawers.Clear();
        }

        private IEnumerable<FieldInfo> GetFieldsOfType(Type type)
        {
            return Content.GetType()
                .GetAllInstanceFields()
                .Where(f => type.IsAssignableFrom(f.FieldType));
        }
        
        public IReadOnlyDictionary<Port, InputDrawer> InputMap => inputMap;
        public IReadOnlyDictionary<Port, OutputDrawer> OutputMap => outputMap;
    }
}