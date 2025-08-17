using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Graphite.Editor.Settings;
using Graphite.Attributes;

namespace Graphite.Editor.CreateNodeSearchWindowProvider
{
    public class DefaultCreateNodeSearchTreeProvider : ICreateNodeSearchTreeProvider
    {
        private static List<CreateNodeEntry> _searchTree;
        
        public List<CreateNodeEntry> CreateSearchTree() => _searchTree ??= BuildSearchTree();

        private static List<CreateNodeEntry> BuildSearchTree()
        {
            var entryBuffer = new List<CreateNodeEntry>(512);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
#if GRAPHITE_ALLOW_CREATION_METHODS
                    foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public 
                                 | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                    {
                        if (IsValidCreationMethod(method))
                        {
                            entryBuffer.Add(GetCreateNodeEntryFromCreationMethod(method));
                        }
                    }
#endif
                    if (typeof(INode).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        entryBuffer.Add(GetCreateNodeEntryFromConstructor(type));
                    }
                }
            }
            return entryBuffer;
        }

        private static bool IsValidCreationMethod(MethodInfo method)
        {
            if (!typeof(INode).IsAssignableFrom(method.ReturnType) || method.GetParameters().Length != 0)
            {
                return false;
            }
            
            return method.GetCustomAttribute(typeof(CreateNodeSearchItem), false) != null;
        }
        
        private static CreateNodeEntry GetCreateNodeEntryFromCreationMethod(MethodInfo method)
        {
            return new CreateNodeEntry(
                ((CreateNodeSearchItem)method.GetCustomAttribute(typeof(CreateNodeSearchItem), false)).Path,
                () => (INode)method.Invoke(null, null)
            );
        }

        private static CreateNodeEntry GetCreateNodeEntryFromConstructor(Type type)
        {
            return new CreateNodeEntry(
                GetSearchItems(type),
                () => (INode)Activator.CreateInstance(type)
            );
        }
        
        private static string GetNodeName(Type type)
        {
            return NodeViewSettings.GetDefaultTitle(type);
        }

        private static string GetSearchItems(Type type)
        {
            return type.GetCustomAttributes(typeof(CreateNodeSearchItem), true)
                .FirstOrDefault() is CreateNodeSearchItem searchItem
                ? searchItem.Path + "/" + GetNodeName(type)
                : GetNodeName(type);
        }
    }
}