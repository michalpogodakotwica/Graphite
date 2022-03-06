using System;

namespace Graphite.Runtime
{
    public interface IOutput 
    {
        internal object Value { get; }
        Type Type { get; }
    }
}