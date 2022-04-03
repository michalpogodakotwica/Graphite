using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.Editor.CreateNodeSearchWindowProvider
{
    public class CreateNodeSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private GraphDrawer.GraphDrawer _graphDrawer;
        private List<SearchTreeEntry> _searchTree;

        public void Initialize(GraphDrawer.GraphDrawer graphDrawer)
        {
            _graphDrawer = graphDrawer;
            
            _searchTree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("Create Node")) };
            var titlePaths = new HashSet<string>();
            
            foreach (var nodeCreationData in graphDrawer.Settings.CreateNodeSearchTreeProvider.CreateSearchTree())
            {
                var nodePath = nodeCreationData.Path;
                var nodeName = nodePath;
                var level = 0;
                var parts = nodePath?.Split('/');

                if (parts is { Length: > 1 })
                {
                    level++;
                    nodeName = parts[^1];
                    var path = "";

                    for (var i = 0; i < parts.Length - 1; i++)
                    {
                        var title = parts[i];
                        path += title;
                        level = i + 1;

                        if (titlePaths.Contains(path)) 
                            continue;
                        
                        _searchTree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                        {
                            level = level
                        });
                        titlePaths.Add(path);
                    }
                }

                _searchTree.Add(new SearchTreeEntry(new GUIContent(nodeName))
                {
                    level = level + 1,
                    userData = nodeCreationData
                });
            }
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return _searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var nodeCreationData = (CreateNodeEntry)searchTreeEntry.userData;
            var node = nodeCreationData.CreateNode.Invoke();
            node.Position = _graphDrawer.ScreenToGraphPosition(context.screenMousePosition);
            _graphDrawer.AddNodes(new List<INode> { node });
            return true;
        }
    }
}