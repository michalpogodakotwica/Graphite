using System;

namespace Attributes
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