using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Graphite.Utils
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<FieldInfo> GetAllInstanceFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Concat(type.GetInheritedPrivateFields());
        }
        
        public static IEnumerable<FieldInfo> GetInheritedPrivateFields(this Type type)
        {
            while (type.BaseType != null)
            {
                type = type.BaseType;
                foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    yield return fieldInfo;
                }
            }
        }
        
        public static IEnumerable<MemberInfo> GetAllInstanceMembers(this Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Concat(type.GetInheritedPrivateMembers());
        }

        private static IEnumerable<MemberInfo> GetInheritedPrivateMembers(this Type type)
        {
            while (type.BaseType != null)
            {
                type = type.BaseType;
                foreach (var memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    yield return memberInfo;
                }
            }
        }
    }
}