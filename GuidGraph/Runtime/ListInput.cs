﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.GuidGraph.Runtime
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
        /*public bool TryGetValue(out T value)
        {
            value = Connection != null ? (T) Connection.Value : default(T); 
            return Connection != null;
        }*/

        public override Type Type => typeof(T);
    }
}