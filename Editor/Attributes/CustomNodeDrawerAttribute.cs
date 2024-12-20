﻿using System;
using Graphite.Editor.ElementDrawerProvider;

namespace Graphite.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomNodeDrawerAttribute : CustomDrawerAttribute
    {
        public CustomNodeDrawerAttribute(Type target, bool useForChildren = true) : base(target, useForChildren)
        {
        }
    }
}