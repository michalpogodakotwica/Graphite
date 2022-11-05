using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.Editor.Settings;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer
{
    public abstract class OutputDrawer
    {
        public readonly NodeDrawer Parent;
        public readonly IOutput Content;
        public readonly Port Port;
        
        public SerializedProperty OutputProperty { get; private set; }

        protected OutputDrawer(IOutput content, NodeDrawer parent, SerializedProperty outputProperty)
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
        public abstract void AddPort();
        public abstract void RemovePort();

        protected NodeViewSettings NodeViewSettings => Parent.NodeViewSettings;
        protected GraphViewSettings GraphViewSettings => Parent.GraphViewSettings;
        protected VisualElement PortContainer => GraphViewSettings.DisplaySettings.ReverseConnectionFlow(Content.Type) ? Parent.inputContainer : Parent.outputContainer;
        protected Direction PortDirection => GraphViewSettings.DisplaySettings.ReverseConnectionFlow(Content.Type) ? Direction.Input : Direction.Output;
    }
}