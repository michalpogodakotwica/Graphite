using System;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime
{
    public interface ISingleInput : IInput
    {
        IOutput Connection { get; }
    }
    
    [Serializable]
    public abstract class Input : ISingleInput
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

        public abstract Type Type { get; }
        IOutput ISingleInput.Connection => Connection;
    }
    
    [Serializable]
    public class Input<T> : Input
    {
        public Input(IOutput connection = null)
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