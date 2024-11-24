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
        public List<CreateNodeEntry> CreateSearchTree()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToArray();

            var creationMethods = assemblies
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                .Where(IsValidCreationMethod)
                .Select(GetCreateNodeEntryFromCreationMethod);

            var nonAbstractNodeTypes = assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(INode).IsAssignableFrom(t) && !t.IsAbstract);

            var constructors = nonAbstractNodeTypes.Select(GetCreateNodeEntryFromConstructor);

            return creationMethods.Concat(constructors).OrderBy(d => d.Path).ToList();
        }

        private static bool IsValidCreationMethod(MethodInfo method)
        {
            return method.GetCustomAttribute(typeof(CreateNodeSearchItem), false) != null &&
                   typeof(INode).IsAssignableFrom(method.ReturnType) && method.GetParameters().Length == 0;
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