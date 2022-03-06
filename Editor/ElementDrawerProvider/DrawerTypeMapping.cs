using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Graphite.Editor.ElementDrawerProvider
{
    public class DrawerTypeMapping<TContent, TDrawerBaseType, TCustomDrawerAttribute> : IElementDrawerMapping
        where TCustomDrawerAttribute : CustomDrawerAttribute
    {
        // ReSharper disable once StaticMemberInGenericType
        private readonly Dictionary<Type, (bool useForChildren, Type)> _contentTypeToDrawerMap;
        
        public DrawerTypeMapping()
        {
            _contentTypeToDrawerMap = new Dictionary<Type, (bool useForChildren, Type)>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var allTypes = assemblies.SelectMany(c => c.GetTypes());
            var drawers = allTypes
                .Where(p => typeof(TDrawerBaseType).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();

            foreach (var drawerType in drawers)
            {
                foreach (var attribute in drawerType.GetCustomAttributes())
                {
                    if (attribute is not TCustomDrawerAttribute customDrawerAttribute)
                        continue;

                    var target = customDrawerAttribute.Target;

                    if (!typeof(TContent).IsAssignableFrom(target))
                        continue;
                    
                    _contentTypeToDrawerMap[target] = (customDrawerAttribute.UseForChildren, drawerType);
                }
            }
        }

        public Type GetDrawerForType(Type contentType)
        {
            if (_contentTypeToDrawerMap.TryGetValue(contentType, out var result))
                return result.Item2;

            var baseType = contentType.BaseType;
            while (baseType != null)
            {
                if (_contentTypeToDrawerMap.TryGetValue(baseType, out result) && result.useForChildren)
                {
                    _contentTypeToDrawerMap[contentType] = result;
                    return result.Item2;
                }

                baseType = baseType.BaseType;
            }
            
            foreach (var @interface in contentType.GetInterfaces())
            {
                if (_contentTypeToDrawerMap.TryGetValue(@interface, out result) && result.useForChildren)
                {
                    _contentTypeToDrawerMap[contentType] = result;
                    return result.Item2;
                }
            }

            return null;
        }
    }
    
    public abstract class CustomDrawerAttribute : Attribute
    {
        public Type Target { get; }
        public bool UseForChildren { get; }

        protected CustomDrawerAttribute(Type target, bool useForChildren = true)
        {
            Target = target;
            UseForChildren = useForChildren;
        }
    }
}