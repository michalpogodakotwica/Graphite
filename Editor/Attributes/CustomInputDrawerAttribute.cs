using System;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;

namespace com.michalpogodakotwica.graphite.Editor.Attributes
{
    public class CustomInputDrawerAttribute : CustomDrawerAttribute
    {
        public CustomInputDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}