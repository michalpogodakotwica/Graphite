using System;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;

namespace com.michalpogodakotwica.graphite.Editor.GraphDrawer.OutputDrawers
{
    public class CustomOutputDrawerAttribute : CustomDrawerAttribute
    {
        public CustomOutputDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}