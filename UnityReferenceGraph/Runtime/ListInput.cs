using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime
{
    public interface IListInput : IInput
    {
        IEnumerable<IOutput> Connections { get; }
    }
    
    public abstract class ListInput : IListInput
    {
        [SerializeReference]
        protected List<IOutput> Connections = new();
        
        public abstract Type Type { get; }
        
        IEnumerable<IOutput> IListInput.Connections => Connections;
    }
    
    [Serializable]
    public class ListInput<T> : ListInput
    {
        public IEnumerable<T> GetValues()
        {
            foreach (var connection in Connections)
            {
                if (connection is T casted)
                    yield return casted;
            }
        }

        public override Type Type => typeof(T);
    }
}