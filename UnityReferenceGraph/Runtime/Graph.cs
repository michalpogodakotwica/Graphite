using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime
{
    [Serializable]
#if ODIN_INSPECTOR
    [DrawWithUnity]
#endif
    public class Graph : IGraph
    {
        [SerializeReference]
        private List<INode> _nodes;

        private bool _wasInitialized;

        public Graph(List<INode> nodes)
        {
            _nodes = nodes;
        }

        private void Initialize()
        {
            if (_wasInitialized)
                return;

            _nodes ??= new List<INode>();

            _wasInitialized = true;
            foreach (var node in _nodes)
                node.Initialize();
        }

        public IEnumerable<INode> Nodes()
        {
            return _nodes;
        }

        public IEnumerator<INode> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }
    }
}