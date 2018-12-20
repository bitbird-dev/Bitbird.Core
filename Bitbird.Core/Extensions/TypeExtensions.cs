using Bitbird.Core.Json.JsonApi.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNonStringEnumerable(this Type type)
        {
            return type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsPrimitiveOrString(this Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        public static string GetJsonApiClassName(this Type type)
        {
            var customName = type.GetTypeInfo().GetCustomAttribute<JsonApiClassAttribute>();
            string typeName = (customName != null) ? customName.Name : type.Name;
            typeName = typeName.Trim();
            return typeName.ToLowerInvariant();
        }
    }
}
