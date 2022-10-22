﻿using System;
using System.Linq;
using Attributes;
using UnityEditor;

namespace com.michalpogodakotwica.graphite.Editor.Settings
{
    public class NodeViewSettings
    {
        public string NodeTitle;

        public static NodeViewSettings Default(INode node)
        {
            return new NodeViewSettings
            {
                NodeTitle = GetDefaultTitle(node.GetType())
            };
        }
        
        public static string GetDefaultTitle(Type nodeType)
        {
            return nodeType.GetCustomAttributes(typeof(NodeTitleAttribute), true)
                .FirstOrDefault() is NodeTitleAttribute titleAttribute
                ? titleAttribute.Title
                : ObjectNames.NicifyVariableName(nodeType.Name);
        }
    }
}