using System;
using System.Collections.Generic;
using Storyboards.PortInversion;
using UnityEngine;
using VContainer;

namespace Storyboards
{
    public abstract class RunableNode : Node, IDisposable, IRuntimeStylable
    {
        [SerializeReference] 
        private InvertedInput<Action> _trigger;

        [Inject]
        private StoryboardRunner _runner;
        
        private bool _isRunning;
        
        [Inject]
        private void Inject()
        {
            _trigger.Add(Run);
        }

        public void Dispose()
        {
            _trigger.Remove(Run);
        }
        
        private void Run()
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            
            _runner.AddRunningNode(this);
            OnRun();
        }
        
        protected abstract void OnRun();
        
        protected void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            IsRunning = false;
            
            _runner.RemoveRunningNode(this);
        }
        
        protected bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning == value)
                {
                    return;
                }
                _isRunning = value;
                
#if UNITY_EDITOR
                UpdateStyles?.Invoke(Styles);
#endif
            }
        }
        
#if UNITY_EDITOR
        public event Action<IEnumerable<string>> UpdateStyles;
        
        public IEnumerable<string> Styles
        {
            get
            {
                if (IsRunning)
                {
                    yield return "running";
                }
            }
        }
#endif
    }
}