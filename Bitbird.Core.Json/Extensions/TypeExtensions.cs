using Bitbird.Core.Json.JsonApi.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bitbird.Core.Json.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns true if the passed type implements <see cref="IEnumerable{T}"/> and is not a string.
        /// Explicitly checks for the generic interface, <see cref="IEnumerable"/> is not enough.
        /// </summary>
        /// <param name="type">The type to check. Must not be null.</param>
        /// <returns>Whether the passed type meets the spec.</returns>
        public static bool IsNonStringEnumerable(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type), $"{nameof(TypeExtensions)}.{nameof(IsNonStringEnumerable)}: The passed {nameof(type)} was null.");

            return type != typeof(string)
                   // get all interfaces that this type implements and look for IEnumerable<> in those interfaces and the type itself
                   && new [] { type }.Concat(type.GetInterfaces()).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
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
