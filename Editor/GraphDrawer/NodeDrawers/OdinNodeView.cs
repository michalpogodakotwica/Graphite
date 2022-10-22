#if ODIN_INSPECTOR
using com.michalpogodakotwica.graphite.Editor.Attributes;
using com.michalpogodakotwica.graphite.Editor.GraphDrawer.NodeDrawers;
using com.michalpogodakotwica.graphite.Editor.Settings;
using UnityEditor;
using UnityEngine.UIElements;

namespace Graphite.Editor.GraphDrawer.NodeDrawers
{
	[CustomNodeDrawer(typeof(INode))]
    public class OdinNodeView : NodeDrawer
    {
        public OdinNodeView(INode content, com.michalpogodakotwica.graphite.Editor.GraphDrawer.GraphDrawer parent, SerializedProperty contentSerializedProperty, NodeViewSettings nodeViewSettings) 
            : base(content, parent, contentSerializedProperty, nodeViewSettings)
        {
        }

        public override void AddNode()
        {
            base.AddNode();
            DrawContent();
            RefreshPorts();
        }

        private void DrawContent()
        {
            var tree = Sirenix.OdinInspector.Editor.PropertyTree.Create(Content, ContentSerializedProperty.serializedObject);
            extensionContainer.Add(new IMGUIContainer(() =>
            {
                Sirenix.Utilities.Editor.GUIHelper.PushLabelWidth(100);
                tree.Draw(false);
                Sirenix.Utilities.Editor.GUIHelper.PopLabelWidth();
            }) { name = "OdinTree" });
            RefreshExpandedState();
        }
    }
}
#endif