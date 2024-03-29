﻿using System;
using com.michalpogodakotwica.graphite.Editor.ElementDrawerProvider;

namespace com.michalpogodakotwica.graphite.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomNodeDrawerAttribute : CustomDrawerAttribute
    {
        public CustomNodeDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}