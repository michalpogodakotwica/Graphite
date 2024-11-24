using System;

namespace Graphite.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeTitleAttribute : Attribute
    {
        public readonly string Title;

        public NodeTitleAttribute(string title)
        {
            Title = title;
        }
    }
}