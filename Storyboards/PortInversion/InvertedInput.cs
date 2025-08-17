using System;
using System.Collections.Generic;
using Graphite.UnityReferenceGraph;

namespace Storyboards.PortInversion
{
    [Serializable]
    public class InvertedInput<T> : Input<List<T>>
    {
        public void Add(T element)
        {
            if (TryGetValue(out var content))
            {
                content.Add(element);
            }
        }

        public void Remove(T element)
        {
            if (TryGetValue(out var content))
            {
                content.Remove(element);
            }
        }
    }
}