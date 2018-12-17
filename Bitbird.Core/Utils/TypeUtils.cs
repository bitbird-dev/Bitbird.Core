using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Utils
{
    public static class TypeUtils
    {
        public static bool IsNonStringEnumerable(this Type type)
        {
            return type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsPrimitiveOrString(this Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        public static object GetValueFast(this PropertyInfo propertyInfo, object data)
        {
            return propertyInfo.GetValue(data);
        }

        public static bool JsonIsIgnoredIfNull(this PropertyInfo propertyInfo)
        {
            var jsonPropAttr = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            return jsonPropAttr?.NullValueHandling == NullValueHandling.Ignore;
        }
        
    }
}
