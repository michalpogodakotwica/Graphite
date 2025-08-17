using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Storyboards
{
    [Serializable]
    public abstract class Node : INode
    {
        [SerializeField, HideInInspector]
        private Vector2 _position;

        [SerializeField, HideInInspector, JsonIgnore] // Json ignore ensures the value isn't passed to copied nodes
        private string _guid = Guid.NewGuid().ToString();

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
        
        public abstract void InitializeOutputs();
        
        public Guid Guid => Guid.Parse(_guid);
    }
}