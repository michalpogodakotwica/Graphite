#pragma warning disable 0414 // private field assigned but not used.

using System;
using Graphite.Attributes;
using UnityEngine;

namespace Storyboards.StickyNote
{
    [Serializable, CreateNodeSearchItem("Sticky Note")]
    public class StickyNoteNode : Node
    {
        [SerializeField, Multiline] 
        private string _contents = "Text";
        
        public override void InitializeOutputs()
        {
        }
    }
}