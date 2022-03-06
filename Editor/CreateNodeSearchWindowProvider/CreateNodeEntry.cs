using System;
using Graphite.Runtime;

namespace Graphite.Editor.CreateNodeSearchWindowProvider
{
    public readonly struct CreateNodeEntry
    {
        public readonly string Path;
        public readonly Func<INode> CreateNode;

        public CreateNodeEntry(string path, Func<INode> createNode)
        {
            Path = path;
            CreateNode = createNode;
        }
    }
}