using System;
using System.Collections.Generic;

namespace Graphite.Runtime
{
    public interface IInput
    {
        Type Type { get; }
        IEnumerable<IOutput> Connections { get; }
        void Connect(IOutput other);
        void Disconnect(IOutput other);
    }
}