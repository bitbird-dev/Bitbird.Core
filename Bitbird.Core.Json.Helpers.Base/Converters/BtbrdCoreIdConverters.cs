using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.Base.Converters
{
    public static class BtbrdCoreIdConverters
    {
        private static Dictionary<Type, BtbrdCoreIdConverter> Converters { get; } = new Dictionary<Type, BtbrdCoreIdConverter>();

        public static void AddConverter(BtbrdCoreIdConverter converter)
        {
            if (Converters.ContainsKey(converter.IdType))
            {
                Converters.Remove(converter.IdType);
            }
            Converters.Add(converter.IdType, converter);
        }

        public static string ConvertToString(object obj)
        {
            if (obj == null) { return null; }
            if (Converters.TryGetValue(obj.GetType(), out var converter))
            {
                return converter.ConvertToString(obj);
            }
            throw new Exception($"Error While converting an object of type {obj.GetType().Name} to string. Please register a converter to BtbrdCoreIdConverters during startup of your application.");
        }

        public static T ConvertFromString<T>(string obj)
        {
            if (Converters.TryGetValue(typeof(T), out var converter))
            {
                return (T)converter.ConvertFromString(obj);
            }
            throw new Exception($"Error While converting an object of type {typeof(T).Name} to string. Please register a converter to BtbrdCoreIdConverters during startup of your application.");
        }

        public static object ConvertFromString(string obj, Type t)
        {
            if (Converters.TryGetValue(t, out var converter))
            {
                return converter.ConvertFromString(obj);
            }
            throw new Exception($"Error While converting an object of type {t.Name} to string. Please register a converter to BtbrdCoreIdConverters during startup of your application.");
        }
    }
}
