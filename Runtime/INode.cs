using UnityEngine;

namespace Graphite.Runtime
{
    public interface INode
    {
        void InitializeOutputs();
        public Vector2 Position { get; set; }
    }
}