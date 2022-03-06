using System;
using System.Collections.Generic;
using Graphite.Editor.ElementDrawerProvider;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using Graphite.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Graphite.Editor.NodeInspector
{
	[CustomEditor(typeof(NodeInspector))]
    public class InspectorEditor : UnityEditor.Editor
    {
	    private static readonly DrawerTypeMapping<INode, INodeInspector, CustomNodeInspectorDrawerAttribute> InspectorDrawerTypeMapping = new();
	    
	    private readonly List<(INodeInspector inspector, VisualElement container, NodeDrawer nodeView)> _inspectors = new();
	    private NodeInspector NodeInspector => (NodeInspector)target;

	    private VisualElement _multiSelectionMessage;
	    private VisualElement _nothingSelectedMessage;
        private VisualElement _mainContainer;

        public override VisualElement CreateInspectorGUI()
        {
	        _multiSelectionMessage = new Label("Multi-node editing not supported");
	        _nothingSelectedMessage = new Label("Select a node to open inspector");
	        
	        _mainContainer = new VisualElement();
	        
	        foreach (var inspectorDrawer in _inspectors)
	        {
		        inspectorDrawer.inspector.Dispose();
	        }
	        _inspectors.Clear();

	        foreach (var nodeView in Selection.Selected)
	        {
		        var inspector = CreateInspector(nodeView);
		        var container = inspector.CreateInspectorGUI(nodeView);
		        _inspectors.Add((inspector, container, nodeView));
	        }

	        UpdateContent();
	        
	        Selection.OnNodeSelected += OnNodeSelected;
	        Selection.OnNodeDeselected += OnNodeDeselected;
	        
	        return _mainContainer;
        }

        private static INodeInspector CreateInspector(NodeDrawer nodeDrawer)
        {
	        var inspectorDrawerType = InspectorDrawerTypeMapping.GetDrawerForType(nodeDrawer.Content.GetType());
	        return (INodeInspector) Activator.CreateInstance(inspectorDrawerType);
        }

        private void OnNodeSelected(NodeDrawer nodeDrawer)
        {
	        var inspector = CreateInspector(nodeDrawer);
	        var container = inspector.CreateInspectorGUI(nodeDrawer);
	        _inspectors.Add((inspector, container, nodeDrawer));
	        
	        UpdateContent();
        }
        
        private void OnNodeDeselected(NodeDrawer node)
        {
	        for (var i = _inspectors.Count - 1; i >= 0; i--)
	        {
		        var (inspector, _, nodeView) = _inspectors[i];
		        if (nodeView != node)
			        continue;
			        
		        _inspectors.RemoveAt(i);
		        inspector.Dispose();
		        break;
	        }
	        
	        UpdateContent();
        }

        private void UpdateContent()
        {
	        _mainContainer.Clear();
	        
	        switch (_inspectors.Count)
	        {
		        case 0:
			        _mainContainer.Add(_nothingSelectedMessage);
			        NodeInspector.name = "Node Inspector";
			        break;
		        case 1:
			        var (_, container, nodeView) = _inspectors[0];
			        _mainContainer.Add(container);
			        NodeInspector.name = nodeView.NodeViewSettings.NodeTitle;
			        break;
		        case > 1:
			        _mainContainer.Add(_multiSelectionMessage);
			        NodeInspector.name = "Node Inspector";
			        break;
	        }
        }
    }
}