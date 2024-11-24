using System;
using UnityEngine;

namespace Graphite.GuidGraph.Runtime
{
    [Serializable]
    public class Output : IOutput
    {
        [SerializeField] 
        private string _guid = Guid.NewGuid().ToString();
        
        public void Initialize<T>(Func<T> getValue)
        {
            GetValue = () => getValue.Invoke();
            Type = typeof(T);
        }

        public Type Type { get; private set; }
        public Func<object> GetValue { get; set; }
        object IOutput.Value => GetValue.Invoke();
        public Guid Guid => Guid.Parse(_guid);
    }
}