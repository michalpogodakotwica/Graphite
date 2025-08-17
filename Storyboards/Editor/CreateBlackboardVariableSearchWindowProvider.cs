using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Storyboards.Editor
{
    public class CreateBlackboardVariableSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private StoryboardDrawer _drawer;
        private List<SearchTreeEntry> _searchTree;
        
        public void Initialize(StoryboardDrawer drawer)
        {
            _drawer = drawer;
            
            var graph = drawer.EditorWindow.Graph as Storyboard;
            
            _searchTree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("Create Variable")) };
            
            foreach (var type in graph.BlackboardVariableTypes())
            {
                _searchTree.Add(new SearchTreeEntry(new GUIContent(type.Name))
                {
                    level = 1,
                    userData = type
                });
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return _searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var type = (Type)searchTreeEntry.userData;
            _drawer.AddVariable(type);
            return true;
        }
    }
}