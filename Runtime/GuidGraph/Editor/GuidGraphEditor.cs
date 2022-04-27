using System.Collections.Generic;
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
            Load();
        }

        private void CreateToolbar()
        {
            var root = rootVisualElement;
            _toolbar = new Toolbar();
            root.Add(_toolbar);

            var saveButton = new Button(Save)
            {
                text = "Save"
            };
            _toolbar.Add(saveButton);
            
            var loadButton = new Button(Load)
            {
                text = "Load"
            };
            _toolbar.Add(loadButton);
        }

        private void ClearToolbar()
        {
            _toolbar.RemoveFromHierarchy();
        }
        
        private void Save()
        {
            var serializedNodes = JsonConvert.SerializeObject(_nodes, Formatting.Indented, Runtime.Graph.Settings);
            GraphProperty.FindPropertyRelative("GraphData").stringValue = serializedNodes;
            EditorUtility.SetDirty(GraphProperty.serializedObject.targetObject);
            GraphProperty.serializedObject.ApplyModifiedProperties();
        }
        
        private void Load()
        {
            var castedGraph = (GuidGraph.Runtime.Graph)Graph;
            castedGraph.Initialize();
            LoadFromRuntimeValue(castedGraph.Nodes);
        }

        private void LoadFromStringData(string nodesSerializedData)
        {
            _nodes = JsonConvert.DeserializeObject<List<GuidGraph.Runtime.INode>>(nodesSerializedData, Runtime.Graph.Settings);
        }
        
        private void LoadFromRuntimeValue(List<GuidGraph.Runtime.INode> nodesSerializedData)
        {
            _nodes = nodesSerializedData;
        }
    }
}