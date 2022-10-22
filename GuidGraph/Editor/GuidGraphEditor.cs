using System.Collections.Generic;
using System.Linq;
using com.michalpogodakotwica.graphite.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.michalpogodakotwica.graphite.GuidGraph.Editor
{
    public class GuidGraphEditor : GraphEditorWindow
    {
        [SerializeReference]
        private List<GuidGraph.Runtime.INode> _nodes = new();

        private Toolbar _toolbar;
        
        protected override void OnGraphDrawerCreated()
        {
            base.OnGraphDrawerCreated();
            CreateToolbar();
        }

        protected override void OnGraphDrawerCleared()
        {
            base.OnGraphDrawerCleared();
            ClearToolbar();
        }

        protected override void OnGraphLoadedFromProperty()
        {
            base.OnGraphPropertyLoaded();
        }

        private void CreateToolbar()
        {
            var root = rootVisualElement;
            _toolbar = new Toolbar();
            root.Add(_toolbar);

            var exportButton = new Button(Export)
            {
                text = "Export (Save)"
            };
            _toolbar.Add(exportButton);
            
            var importButton = new Button(Import)
            {
                text = "Import (Reload)"
            };
            _toolbar.Add(importButton);
            
            
            var loadRuntime = new Button(() =>
            {
                var castedGraph = (GuidGraph.Runtime.Graph)Graph;
                _nodes = castedGraph.Nodes;
                foreach (var node in _nodes)
                    node.Initialize();
            })
            {
                text = "Load runtime "
            };
            _toolbar.Add(loadRuntime);
            
            var inspect = new Button(() => Selection.activeObject = this)
            {
                text = "Inspect"
            };
            _toolbar.Add(inspect);
        }

        private void ClearToolbar()
        {
            _toolbar.RemoveFromHierarchy();
        }
        
        private void Export()
        {
            var serializedNodes = JsonConvert.SerializeObject(_nodes, Formatting.Indented, Runtime.Graph.Settings);
            GraphProperty.FindPropertyRelative("GraphData").stringValue = serializedNodes;
            EditorUtility.SetDirty(GraphProperty.serializedObject.targetObject);
            GraphProperty.serializedObject.ApplyModifiedProperties();
        }
        
        private void Import()
        {
            var castedGraph = (GuidGraph.Runtime.Graph)Graph;
            castedGraph.Initialize();
            _nodes = castedGraph.Nodes;
            
            RedrawGraph();
        }
    }
}