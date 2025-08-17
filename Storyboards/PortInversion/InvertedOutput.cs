using System;
using System.Collections.Generic;
using Graphite.UnityReferenceGraph;

namespace Storyboards.PortInversion
{
    [Serializable]
    public class InvertedOutput<T> : Output<List<T>>
    {
        public List<T> Content { get; private set; } 
        
        public void Initialize()
        {
            Content = new List<T>();
            Initialize(() => Content);
        }
    }
}