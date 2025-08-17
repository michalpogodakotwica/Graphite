using System;
using System.Collections.Generic;
using Graphite;
using UnityEngine;

namespace Storyboards
{
    [Serializable]
    public class Storyboard : IGraph
    {
        [SerializeReference] 
        private List<Node> _nodes = new();

        [SerializeReference]
        private List<IBlackboardVariable> _blackboard = new();
        
        public IReadOnlyList<IBlackboardVariable> Blackboard => _blackboard;
        public IEnumerable<Node> Nodes => _nodes;

        public IEnumerable<Type> BlackboardVariableTypes()
        {
            yield return typeof(bool);
            yield return typeof(int);
            yield return typeof(float);
            yield return typeof(Transform);
        }
    }
}