using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphite.Runtime.Ports
{
    [Serializable]
    public abstract class MultiInput : IInput
    {
        [SerializeReference]
        protected List<IOutput> Connections = new(); 
        
        public IEnumerator<IOutput> GetEnumerator()
        {
            return Connections.GetEnumerator();
        }
        
        public void Connect(IOutput other)
        {
            Connections.Add(other);
        }

        public void Disconnect(IOutput other)
        {
            Connections.Remove(other);
        }

        IEnumerable<IOutput> IInput.Connections => Connections;
        
        public abstract Type Type { get; }
    }
    
    [Serializable]
    public class MultiInput<T> : MultiInput
    {
        public override Type Type => typeof(T);
        
        public bool TryGetValue(out List<T> value)
        {
            value = Connections?.Cast<T>().ToList(); 
            return value != null;
        }
    }
}