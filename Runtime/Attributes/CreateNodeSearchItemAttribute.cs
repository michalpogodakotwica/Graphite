using System;

namespace Graphite.Runtime.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CreateNodeSearchItem : Attribute
    {
        public readonly string Path;

        public CreateNodeSearchItem(string path)
        {
            Path = path;
        }
    }
}