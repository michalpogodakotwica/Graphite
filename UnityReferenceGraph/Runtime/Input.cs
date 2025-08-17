using System;
using UnityEngine;

namespace Graphite.UnityReferenceGraph
{
    public interface ISingleInput : IInput
    {
        IOutput Connection { get; }
    }
    
    [Serializable]
    public class Input<T> : ISingleInput
    {
        [SerializeReference]
        protected IOutput Connection; 

        public Input()
        {
        }

        public Input(IOutput connection)
        {
            if (connection.Type == typeof(T))
                Connection = connection as Output<T>;
        }

        public bool TryGetValue(out T value)
        {
            value = Connection != null ? (T) Connection.Value : default; 
            return Connection != null;
        }

        public Type Type => typeof(T);
        IOutput ISingleInput.Connection => Connection;
    }
}