using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.michalpogodakotwica.graphite.Editor.CopyPasteHandler
{
    [Serializable]
    public class DefaultCopyPasteGraphData
    {
        [SerializeReference] 
        public List<INode> Nodes;
    }
}