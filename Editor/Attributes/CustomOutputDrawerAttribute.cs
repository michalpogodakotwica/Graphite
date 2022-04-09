using System;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;

namespace com.michalpogodakotwica.graphite.Editor.Attributes
{
    public class CustomOutputDrawerAttribute : CustomDrawerAttribute
    {
        public CustomOutputDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}