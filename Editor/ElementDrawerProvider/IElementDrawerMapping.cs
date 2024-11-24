using System;

namespace Graphite.Editor.ElementDrawerProvider
{
    public interface IElementDrawerMapping
    {
        Type GetDrawerForType(Type contentType);
    }
}