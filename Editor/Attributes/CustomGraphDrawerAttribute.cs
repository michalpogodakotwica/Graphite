using System;
using Graphite.Editor.ElementDrawerProvider;

namespace Graphite.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomGraphDrawerAttribute : CustomDrawerAttribute
    {
        public CustomGraphDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}