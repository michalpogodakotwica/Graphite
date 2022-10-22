using System;

namespace Attributes
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