using System;
using System.Collections.Generic;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEngine;

namespace Graphite.Editor.NodeInspector
{
    public static class Selection
    {
        public static event Action<NodeDrawer> OnNodeSelected;
        public static event Action<NodeDrawer> OnNodeDeselected;

        private static readonly List<NodeDrawer> _selected = new();
        
        private static NodeInspector _inspector;

        public static void SelectNode(NodeDrawer nodeDrawer)
        {
            _selected.Add(nodeDrawer);

            OpenNodeInspector();

            OnNodeSelected?.Invoke(nodeDrawer);
        }

        public static void OpenNodeInspector()
        {
            if (_inspector == null)
            {
                _inspector = ScriptableObject.CreateInstance<NodeInspector>();
            }

            if (UnityEditor.Selection.activeObject != _inspector)
            {
                UnityEditor.Selection.activeObject = _inspector;
            }
        }
        
        public static void DeselectNode(NodeDrawer nodeDrawer)
        {
            _selected.Remove(nodeDrawer);
            OnNodeDeselected?.Invoke(nodeDrawer);
        }
        
        public static IReadOnlyList<NodeDrawer> Selected => _selected;
    }
}