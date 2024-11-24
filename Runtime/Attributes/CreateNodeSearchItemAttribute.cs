using System;

namespace Graphite.Attributes
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