using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Graphite.Editor.Utils
{
    public class UnitySerializationResemblingJsonContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);

            jsonProperty.Readable = true;
            jsonProperty.Writable = true;

            if (!member.GetCustomAttributes().Any(a => a is SerializeReference)) 
                return jsonProperty;
            
            if(typeof(IList).IsAssignableFrom(jsonProperty.PropertyType))
                jsonProperty.ItemIsReference = true;
            else
                jsonProperty.IsReference = true;

            return jsonProperty;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var typeAndPublicMembers = objectType
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(f => f.IsPublic && f.GetCustomAttributes().All(attribute => attribute is not NonSerializedAttribute)
                            || f.GetCustomAttributes().Any(attribute1 => attribute1 is SerializeField or SerializeReference))
                .Cast<MemberInfo>();

            if (objectType.BaseType == null)
            {
                return typeAndPublicMembers.ToList();
            }
            
            var inheritedFields = GetInheritedPrivateMembers(objectType.BaseType);
            return typeAndPublicMembers.Concat(inheritedFields).ToList();
        }

        private static IEnumerable<MemberInfo> GetInheritedPrivateMembers(Type objectType)
        {
            var privateMembers = objectType
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsPrivate && f.GetCustomAttributes()
                                .Any(attribute1 => attribute1 is SerializeField or SerializeReference))
                .Cast<MemberInfo>();
            
            return objectType.BaseType == null
                ? privateMembers 
                : privateMembers.Concat(GetInheritedPrivateMembers(objectType.BaseType));
        }
    }
}