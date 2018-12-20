using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static object GetValueFast(this PropertyInfo propertyInfo, object obj)
        {
            return propertyInfo.GetValue(obj);
        }

        public static void SetValueFast(this PropertyInfo propertyInfo, object obj, object value)
        {
            propertyInfo.SetValue(obj, value);
        }

        public static bool JsonIsIgnoredIfNull(this PropertyInfo propertyInfo)
        {
            var jsonPropAttr = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            return jsonPropAttr?.NullValueHandling == NullValueHandling.Ignore;
        }
    }
}
