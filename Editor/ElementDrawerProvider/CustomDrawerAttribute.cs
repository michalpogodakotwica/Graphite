using System;

namespace Graphite.Editor.ElementDrawerProvider
{
    public abstract class CustomDrawerAttribute : Attribute
    {
        public Type Target { get; }
        public bool UseForChildren { get; }

        protected CustomDrawerAttribute(Type target, bool useForChildren = true)
        {
            Target = target;
            UseForChildren = useForChildren;
        }
    }
}