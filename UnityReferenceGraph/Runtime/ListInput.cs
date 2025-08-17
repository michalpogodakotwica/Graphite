using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphite.UnityReferenceGraph
{
    public interface IListInput : IInput
    {
        IEnumerable<IOutput> Connections { get; }
    }
    
    [Serializable]
    public class ListInput<T> : IListInput
    {
        [SerializeReference]
        protected List<IOutput> Connections = new();

        public IEnumerable<T> GetValues()
        {
            foreach (var connection in Connections)
            {
                if (connection.Value is T casted)
                    yield return casted;
            }
        }

        IEnumerable<IOutput> IListInput.Connections => Connections;
        public Type Type => typeof(T);
    }
}