using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Extensions
{
    public static class PropertyInfoExtensions
    {
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
