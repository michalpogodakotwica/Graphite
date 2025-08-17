using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphite.UnityReferenceGraph
{
    [Serializable]
    public class Graph : IGraph
    {
        [SerializeReference]
        private List<INode> _nodes;

        public Graph(List<INode> nodes)
        {
            _nodes = nodes;
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