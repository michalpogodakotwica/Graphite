using System.Collections.Generic;

namespace com.michalpogodakotwica.graphite.Editor.CreateNodeSearchWindowProvider
{
    public interface ICreateNodeSearchTreeProvider
    {
        List<CreateNodeEntry> CreateSearchTree();
    }
}