using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphite.GuidGraph.Runtime
{ 
    public interface IListInput : IInput
    {
        IEnumerable<Guid> Connections { get; }
    }
    
    [Serializable]
    public abstract class ListInput : IListInput
    {
        [SerializeField] 
        protected List<string> Connections;
        
        public void Connect(Output other)
        {
            Connections.Add(other.Guid.ToString());
        }

        public void Disconnect(Output other)
        {
            Connections.Remove(other.Guid.ToString());
        }

        public abstract Type Type { get; }

        IEnumerable<Guid> IListInput.Connections => Connections.Select(Guid.Parse);
    }
    
    [Serializable]
    public class ListInput<T> : ListInput
    {
        public override Type Type => typeof(T);
    }
}