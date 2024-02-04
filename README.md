Graphite is a graph editor built upon Unity's Experimental GraphView that focuses on working with serialized data structures.

## Features
- Blazing fast in both displaying and modifying data
- Separated and replaceable serialization layer - use built-in UnityReference-based serialization or implement your own. No runtime code tied to Unity API - make your graphs Unity-agnostic if you wish
- Simple and ready-to-use UnityReferenceGraph
- No runtime code hidden inside the magic box
- Comfortable to code
  - Define your hierarchy on your own - implement interfaces to create nodes no need to extend classes
  - Easy to implement - initialize your output ports, implement position property and you are ready to go
  - Lightweight and straightforward port classes
  - Supports node inheritance
- Comfortable to work with
  - Undo, redo support
  - Copy-paste
  - Intuitive controls
  - Strongly typed ports that validate your input
  - Node inspector for a more detailed view
  - Minimap
  - Customizable visuals with GraphViewSettings attribute

## Hello World! with UnityReference based serialization
```cs
using System;
using System.Linq;
using Attributes;
using com.michalpogodakotwica.graphite.UnityReferenceGraph.Runtime;
using UnityEngine;

namespace HelloWorld
{
    public abstract class BaseNode : INode
    {
        [SerializeField, HideInInspector] 
        private Vector2 _position;
    
        public Vector2 Position 
        {
            get => _position;
            set => _position = value;
        }

        public abstract void Initialize();
    }

    [Serializable]
    [CreateNodeSearchItem("Log"), NodeTitle("Log")]
    public class LogInfoNode : BaseNode
    {
        [SerializeField, ShowInNode] 
        private string _message;

        [SerializeField, SerializeReference]
        private Output _trigger = new();
    
        public override void Initialize()
        {
            _trigger.Initialize<Action>(() => Log);
        }

        private void Log()
        {
            UnityEngine.Debug.Log(_message);
        }
    }

    [Serializable]
    [CreateNodeSearchItem("Start"), NodeTitle("On Start")]
    public class StartNode : BaseNode
    {
        [SerializeField]
        private Input<Action> _onStart = new();
    
        public void Start()
        {
            if (_onStart.TryGetValue(out Action value))
            {
                value.Invoke();
            }
        }

        public override void Initialize()
        {
        }
    }

    public class Example : MonoBehaviour
    {
        [SerializeField]
        [GraphViewSettings(graphStyleSheetsResourcePaths: "Connections")]
        private Graph _test;

        private void Start()
        {
            var startNodes = _test.Nodes().Where(n => n is StartNode).Cast<StartNode>();
            foreach (var startNode in startNodes)
            {
                startNode.Start();
            }
        }
    }
}
```
