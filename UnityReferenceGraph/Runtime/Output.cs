using System;

namespace Graphite.UnityReferenceGraph
{
    [Serializable]
    public class Output<T> : IOutput
    {
        public Func<T> GetValue { get; set; }
        
        public void Initialize(Func<T> getValue)
        {
            GetValue = getValue;
        }

        public T Value => GetValue.Invoke();
        public Type Type => typeof(T);
        object IOutput.Value => Value;
    }
}