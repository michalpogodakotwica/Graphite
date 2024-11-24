using System.Collections.Generic;

namespace Graphite.Editor.CreateNodeSearchWindowProvider
{
    public interface ICreateNodeSearchTreeProvider
    {
        List<CreateNodeEntry> CreateSearchTree();
    }
}