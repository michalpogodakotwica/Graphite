using System;

namespace com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider
{
    public interface IElementDrawerMapping
    {
        Type GetDrawerForType(Type contentType);
    }
}