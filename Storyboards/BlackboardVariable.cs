using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Storyboards
{
    public interface IBlackboardVariable
    {
        string Guid { get; }
        string Name { get; }
        Type Type { get; }
    }
    
    [Serializable]
    public class BlackboardVariable<T> : IBlackboardVariable
    {
        [field: SerializeField, HideInInspector, JsonIgnore]
        public string Guid { get; private set; } = System.Guid.NewGuid().ToString();

        [field: SerializeField]
        public string Name { get; private set; }
    
        private bool _wasInjected;
        private T _value;
        
        public T Value
        {
            get => _wasInjected ? _value : throw new BlackboardVariableNotInitializedException(this);
            private set => _value = value;
        }

        public BlackboardVariable()
        {
            Name = Type.Name;
        }
        
        public void InjectContext(T context)
        {
            _wasInjected = true;
            Value = context;
        }
        
        public Type Type => typeof(T);
    }
}