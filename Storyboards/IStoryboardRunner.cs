using System;

namespace Storyboards
{
    public interface IStoryboardRunner : IDisposable
    {
        public event Action OnBeforePlayed;
        public event Action OnAfterStopped;

        public void Play();
        public void Stop();
        public bool IsPlaying { get; }
    }
}