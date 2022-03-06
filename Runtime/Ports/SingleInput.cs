using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphite.Runtime.Ports
{
    [Serializable]
    public abstract class SingleInput : IInput
    {
        [SerializeReference]
        protected IOutput Connection; 
        
        public void Connect(IOutput other)
        {
            Connection = other;
        }

        public void Disconnect(IOutput other)
        {
            if (Connection == other)
                Connection = null;
        }

        public IEnumerable<IOutput> Connections
        {
            get
            {
                if (Connection != null)
                {
                    yield return Connection;
                }
            }
        }

        public IOutput GetConnection() => Connection;
        public abstract Type Type { get; }
    }
    
    [Serializable]
    public class SingleInput<T> : SingleInput
    {
        public SingleInput(IOutput connection = null)
        {
            if(connection == null)
                return;

            if(connection.Type == typeof(T))
                Connection = connection;
        }
        
        public bool TryGetValue(out T value)
        {
            value = Connection != null ? (T) Connection.Value : default(T); 
            return Connection != null;
        }
        
        public override Type Type => typeof(T);
    }
}