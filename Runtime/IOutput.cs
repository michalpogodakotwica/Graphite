using System;

namespace com.michalpogodakotwica.graphite
{
    public interface IOutput
    {
        object Value { get; }
        Type Type { get; }
    }
}