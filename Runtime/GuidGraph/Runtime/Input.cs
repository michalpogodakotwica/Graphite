using System;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.GuidGraph.Runtime
{
    public interface ISingleInput : IInput
    {
        Guid? Connection { get; }
    }
    
    [Serializable]
    public abstract class Input : ISingleInput
    {
        [SerializeField] 
        private string _guid;
        
        public void Connect(Output other)
        {
            _guid = other.Guid.ToString();
        }

        public void Disconnect(Output other)
        {
            if (Guid.Parse(_guid) == other.Guid)
                _guid = null;
        }

        public abstract Type Type { get; }
        Guid? ISingleInput.Connection => string.IsNullOrEmpty(_guid) ? null : Guid.Parse(_guid);
    }
    
    [Serializable]
    public class Input<T> : Input
    {
        public Input(Output connection = null)
        {
            if(connection == null)
                return;

            if (connection.Type == typeof(T))
                Connect(connection);
        }
        
        /*public bool TryGetValue(out T value)
        {
            value = Connection != null ? (T) Connection.Value : default(T); 
            return Connection != null;
        }*/

        public override Type Type => typeof(T);
    }
}