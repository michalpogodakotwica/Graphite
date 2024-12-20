﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphite.UnityReferenceGraph
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
                if (connection.Value is T casted)
                    yield return casted;
            }
        }

        public override Type Type => typeof(T);
    }
}