using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphite.Runtime
{
    [Serializable]
#if ODIN_INSPECTOR
    [DrawWithUnity]
#endif
    public class Graph : IEnumerable<INode>
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
                node.InitializeOutputs();
        }

        public IEnumerator<INode> GetEnumerator()
        {
            Initialize();
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Initialize();
            return GetEnumerator();
        }
    }
}