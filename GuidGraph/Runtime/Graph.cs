using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Graphite.GuidGraph.Runtime
{
    [Serializable]
    public class Graph : IGraph
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new ContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects
        };

        public string GraphData;
        private List<INode> _nodes = new List<INode>();
        
        public void Initialize()
        {
            _nodes = JsonConvert.DeserializeObject<List<INode>>(GraphData, Settings) ?? new List<INode>();

            foreach (var node in _nodes)
                node.Initialize();
        }

        public List<INode> Nodes => _nodes;

        public IEnumerator<INode> GetEnumerator()
        {
            Initialize();
            return _nodes.GetEnumerator();
        }
    }
}