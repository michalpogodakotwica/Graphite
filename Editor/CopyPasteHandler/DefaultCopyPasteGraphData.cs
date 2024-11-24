using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphite.Editor.CopyPasteHandler
{
    [Serializable]
    public class DefaultCopyPasteGraphData
    {
        [SerializeReference] 
        public List<INode> Nodes;
    }
}