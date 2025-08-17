using Graphite.Editor.Attributes;
using Graphite.Editor.GraphDrawer;
using Graphite.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Storyboards.StickyNote.Editor
{
    [CustomNodeDrawer(typeof(StickyNoteNode))]
    public class StickyNoteDrawer : Graphite.Editor.GraphDrawer.NodeDrawers.NodeDrawer
    {
	    private readonly TextField _contentsField;
	    
        public StickyNoteDrawer(Graphite.INode content, GraphDrawer parent,
            SerializedProperty contentSerializedProperty, NodeViewSettings nodeViewSettings) : base(content, parent,
            contentSerializedProperty, nodeViewSettings)
        {
	        this.Q("node-border").RemoveFromHierarchy();
	        
	        AddToClassList("sticky-note");
	        
	        var visualElement = new VisualElement();
	        Insert(0, visualElement);

	        var uxml = Resources.Load<VisualTreeAsset>("StickyNoteNode");
	        uxml.CloneTree(visualElement);
	        _contentsField = this.Q<TextField>("contents-field");
	        
	        styleSheets.Add(Resources.Load<StyleSheet>("StickyNoteNodeStyleSheet"));
        }
        
		public override void ReassignProperty(SerializedProperty contentSerializedProperty)
		{
			base.ReassignProperty(contentSerializedProperty);
			_contentsField.Unbind();
			_contentsField.BindProperty(contentSerializedProperty.FindPropertyRelative("_contents"));
		}

		public override void AddNode()
		{
			LoadPosition();
			_contentsField.BindProperty(ContentSerializedProperty.FindPropertyRelative("_contents"));
		}

		public override void ClearNode()
		{
			_contentsField.Unbind();
		}
    }
}