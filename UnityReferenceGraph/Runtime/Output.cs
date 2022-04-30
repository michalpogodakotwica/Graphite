using System;

namespace com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime
{
    [Serializable]
    public class Output : IOutput
    {
        public void Initialize<T>(Func<T> getValue)
        {
            GetValue = () => getValue.Invoke();
            Type = typeof(T);
        }

        public Type Type { get; private set; }
        public Func<object> GetValue { get; set; }
        object IOutput.Value => GetValue.Invoke();
    }
}