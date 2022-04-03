using System;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;

namespace com.michalpogodakotwica.graphite.Editor.NodeInspector
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