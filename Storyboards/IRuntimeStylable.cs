using System;
using System.Collections.Generic;

namespace Storyboards
{
    public interface IRuntimeStylable
    {
        #if UNITY_EDITOR
        public event Action<IEnumerable<string>> UpdateStyles;
        public IEnumerable<string> Styles { get; }
        #endif
    }
}