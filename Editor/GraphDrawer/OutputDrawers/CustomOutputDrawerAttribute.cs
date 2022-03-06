using System;
using Graphite.Editor.ElementDrawerProvider;

namespace Graphite.Editor.GraphDrawer.OutputDrawers
{
    public class CustomOutputDrawerAttribute : CustomDrawerAttribute
    {
        public CustomOutputDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}