using System;
using Graphite.Editor.ElementDrawerProvider;

namespace Graphite.Editor.Attributes
{
    public class CustomInputDrawerAttribute : CustomDrawerAttribute
    {
        public CustomInputDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}