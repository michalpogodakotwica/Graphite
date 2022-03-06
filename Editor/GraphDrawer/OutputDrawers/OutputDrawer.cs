using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Editor.Settings;
using Graphite.Runtime.Ports;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Graphite.Editor.GraphDrawer.OutputDrawers
{
    public abstract class OutputDrawer
    {
        public readonly NodeDrawer Parent;
        public readonly Output Content;
        public readonly Port Port;
        
        public SerializedProperty OutputProperty { get; private set; }

        protected OutputDrawer(Output content, NodeDrawer parent, SerializedProperty outputProperty)
        {
            Parent = parent;
            OutputProperty = outputProperty;
            Content = content;
            Port = Port.Create<Edge>(Orientation.Horizontal, PortDirection, Port.Capacity.Multi, content.Type);
            
            foreach (var styleSheet in Parent.GraphViewSettings.DisplaySettings.GraphStyleSheets)
            {
                Port.styleSheets.Add(styleSheet);
            }
            
            Port.portName = outputProperty.displayName;
        }
        
        public virtual void ReassignProperty(SerializedProperty inputProperty)
        {
            OutputProperty = inputProperty;
        }

        public abstract void DrawPort();
        public abstract void ClearPort();

        protected NodeViewSettings NodeViewSettings => Parent.NodeViewSettings;
        protected GraphViewSettings GraphViewSettings => Parent.GraphViewSettings;
        protected VisualElement PortContainer => GraphViewSettings.DisplaySettings.OutputsOnRight ? Parent.outputContainer : Parent.inputContainer;
        protected Direction PortDirection => GraphViewSettings.DisplaySettings.OutputsOnRight ? Direction.Output : Direction.Input;
    }
}