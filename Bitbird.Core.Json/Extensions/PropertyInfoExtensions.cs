using Newtonsoft.Json;
using System.Reflection;

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
            if (propertyInfo.CanWrite)
                propertyInfo.SetValue(obj, value);
        }

        public static bool JsonIsIgnoredIfNull(this PropertyInfo propertyInfo)
        {
            var jsonPropAttr = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            return jsonPropAttr?.NullValueHandling == NullValueHandling.Ignore;
        }
    }
}
