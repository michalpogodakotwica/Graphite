using System;
using Graphite.Editor.ElementDrawerProvider;

namespace Graphite.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomNodeInspectorDrawerAttribute : CustomDrawerAttribute
    {
        public CustomNodeInspectorDrawerAttribute(Type target, bool useForChildren = true) 
            : base(target, useForChildren)
        {
        }
    }
}