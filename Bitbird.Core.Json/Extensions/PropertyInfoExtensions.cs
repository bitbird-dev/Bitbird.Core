using System.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace Bitbird.Core.Json.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static object GetValueFast(this PropertyInfo propertyInfo, object obj)
        {
#if (NET40)
            return propertyInfo.GetValue(obj, null);
#else
            return propertyInfo.GetValue(obj);
#endif
        }

        public static void SetValueFast(this PropertyInfo propertyInfo, object obj, object value)
        {
            if (propertyInfo.CanWrite)
#if (NET40)
                propertyInfo.SetValue(obj, value, null);
#else
            propertyInfo.SetValue(obj, value);
#endif
        }

        public static bool JsonIsIgnoredIfNull(this PropertyInfo propertyInfo)
        {
#if (NET40)
            var jsonPropAttr = propertyInfo.GetCustomAttributes(typeof(JsonPropertyAttribute),false).FirstOrDefault() as JsonPropertyAttribute;
#else
            var jsonPropAttr = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
#endif
            return jsonPropAttr?.NullValueHandling == NullValueHandling.Ignore;
        }
    }
}
