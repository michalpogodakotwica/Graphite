using System;
using System.Collections.Generic;
using Storyboards.Test;
using UnityEngine;
using VContainer.Unity;

namespace Storyboards
{
    public class StoryboardRunner : LifetimeScope, IStoryboardRunner
    {
        public event Action OnBeforePlayed;
        public event Action OnAfterStopped;
        
        [SerializeField]
        private StoryboardAsset _asset;

        [SerializeReference]
        private List<IBlackboardVariableContext> _context;
        
        [SerializeField]
        private bool _autoStart;

        private bool _isGraphStarting;
        private bool _isGraphStopping;
        
        private StoryboardAsset _runtimeAsset;
        private readonly HashSet<RunableNode> _runningNodes = new();

        protected override void Awake()
        {
            base.Awake();
            _runtimeAsset = Instantiate(_asset);
        }

        protected override void OnDestroy()
        {
            Destroy(_runtimeAsset);
            _runtimeAsset = null;
        }
        
        private void Start()
        {
            if (_autoStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }
            
            foreach (var node in _runtimeAsset.Storyboard.Nodes)
            {
                if (node is IUpdateable updateable)
                {
                    updateable.Update(Time.deltaTime);
                }
            }
        }

        public void Play()
        {
            if (IsPlaying)
            {
                return;
            }
            
            if (_isGraphStopping)
            {
                Debug.LogError("Can't start storyboard while it is stopping.", this);
                return;
            }
            
            InitializeGraph();
            
            OnBeforePlayed?.Invoke();
            
            _isGraphStarting = true;
            
            foreach (var node in _runtimeAsset.Storyboard.Nodes)
            {
                if (node is IStartable startable)
                {
                    startable.Start();
                }
            }
            _isGraphStarting = false;
            
            if (_runningNodes.Count == 0)
            {
                Stop();
            }
        }

        private void InitializeGraph()
        {
            foreach (var blackboardVariable in _runtimeAsset.Storyboard.Blackboard)
            {
                var contextVariable = _context.Find(v => v.Guid == blackboardVariable.Guid);
                if (contextVariable == null)
                {
                    Debug.LogWarning($"Blackboard variable '{blackboardVariable.Name}' not found in context. It will not be initialized.");
                    continue;
                }
                contextVariable.InjectInto(blackboardVariable);
            }
            
            foreach (var node in _runtimeAsset.Storyboard.Nodes)
            {
                node.InitializeOutputs();
            }

            foreach (var node in _runtimeAsset.Storyboard.Nodes)
            {
                try
                {
                    Container.Inject(node);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Failed to inject dependencies into nodes of {gameObject.name} story board, node {node}",
                        this);
                    Debug.LogException(e, this);
                    throw;
                }
            }
        }

        public void AddRunningNode(RunableNode runableNode)
        {
            if (!IsPlaying)
            {
                Debug.LogError("Trying to add a running node while the storyboard is not playing.", this);
            }
            
            _runningNodes.Add(runableNode);
        }
        
        public void RemoveRunningNode(RunableNode runableNode)
        {
            _runningNodes.Remove(runableNode);
            Stop();
        }

        public void Stop()
        {
            if (!IsPlaying || _isGraphStopping)
            {
                return;
            }

            if (_isGraphStarting)
            {
                Debug.LogError("Can't stop storyboard while it is starting.", this);
                return;
            }
            
            _isGraphStopping = true;
            foreach (var node in _runtimeAsset.Storyboard.Nodes)
            {
                if (node is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            OnAfterStopped?.Invoke();
            _isGraphStopping = false;
        }
        
        public new void Dispose()
        {
            Stop();
            base.Dispose();
        }
        
        public StoryboardAsset Asset => _asset;
        public StoryboardAsset RuntimeAsset => _runtimeAsset;
        public IReadOnlyList<IBlackboardVariableContext> Context => _context;
        public bool IsPlaying => _isGraphStarting || _runningNodes.Count > 0;
    }
}