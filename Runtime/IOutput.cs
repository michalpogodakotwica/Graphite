using System;

namespace Graphite
{
    public interface IOutput
    {
        object Value { get; }
        Type Type { get; }
    }
}