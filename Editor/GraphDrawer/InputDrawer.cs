using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.Editor.Settings;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer
{
    public abstract class InputDrawer
    {
        public readonly NodeDrawer Parent;
        
        protected readonly IInput Content;
        
        protected SerializedProperty InputProperty { get; private set; }
        
        protected InputDrawer(IInput content, NodeDrawer parent, SerializedProperty inputProperty)
        {
            Parent = parent;
            InputProperty = inputProperty;
            Content = content;
        }

        public abstract void DrawPorts();
        public abstract void ClearPorts();
        public abstract void DrawConnections();

        public virtual void ReassignProperty(SerializedProperty inputProperty)
        {
            InputProperty = inputProperty;
        }

        public abstract bool TryToConnect(OutputDrawer outputDrawer, Edge edge);
        public abstract bool TryToDisconnect(OutputDrawer outputDrawer, Edge edge);

        public NodeViewSettings NodeViewSettings => Parent.NodeViewSettings;
        public GraphViewSettings GraphViewSettings => Parent.GraphViewSettings;
        
        protected VisualElement PortContainer => GraphViewSettings.DisplaySettings.OutputsOnRight ? Parent.inputContainer : Parent.outputContainer;
        protected Direction PortDirection => GraphViewSettings.DisplaySettings.OutputsOnRight ? Direction.Input : Direction.Output;
    }
}