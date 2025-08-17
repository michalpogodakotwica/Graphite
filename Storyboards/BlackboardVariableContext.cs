using System;
using UnityEngine;

namespace Storyboards
{
    public interface IBlackboardVariableContext
    {
        public string Guid { get; }
        
        public void InjectInto(IBlackboardVariable blackboardVariable);
    }
    
    [Serializable]
    public class BlackboardVariableContext<T> : IBlackboardVariableContext
    {
        [field: SerializeField, HideInInspector]
        public string Guid { get; private set; }

        [field: SerializeField]
        public T Value { get; private set; }
        
        public void InjectInto(IBlackboardVariable blackboardVariable)
        {
            if (blackboardVariable is BlackboardVariable<T> variable)
            {
                variable.InjectContext(Value);
            }
            else
            {
                Debug.LogError($"Type mismatch. Cannot inject {typeof(T)} into {blackboardVariable.Type}. " +
                               $"Did you create a new variable with ID of an old variable of a different type?");
            }
        }

        public Type Type => typeof(T);
    }
}