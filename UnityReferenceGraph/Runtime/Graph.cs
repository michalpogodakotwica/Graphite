﻿using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

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

        protected virtual void Initialize()
        {
            if (_wasInitialized)
                return;

            _nodes ??= new List<INode>();

            _wasInitialized = true;
            foreach (var node in _nodes)
                node.Initialize();
        }

        public void Reset()
        {
            _wasInitialized = false;
        }

        public IEnumerable<INode> Nodes()
        {
            Initialize();
            return _nodes;
        }

        public IEnumerator<INode> GetEnumerator()
        {
            Initialize();
            return _nodes.GetEnumerator();
        }
    }
}