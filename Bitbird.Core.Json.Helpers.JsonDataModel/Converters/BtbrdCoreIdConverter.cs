using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.JsonDataModel.Converters
{
    public abstract class BtbrdCoreIdConverter
    {
        public Type IdType { get; private set; }

        internal BtbrdCoreIdConverter(Type idType)
        {
            IdType = idType;
        }

        public abstract string ConvertToString(object obj);
        public abstract object ConvertFromString(string obj);
    }

    public sealed class BtbrdCoreIdConverter<T> : BtbrdCoreIdConverter
    {
        private Func<T, string> ToStringConverter { get; set; }
        private Func<string, T> ToIdConverter { get; set; }

        public BtbrdCoreIdConverter(Func<T, string> toStringConverter, Func<string, T> toIdConverter) : base(typeof(T))
        {
            ToStringConverter = toStringConverter ?? throw new ArgumentNullException("toStringConverter", "Converter must not be null.");
            ToIdConverter = toIdConverter ?? throw new ArgumentNullException("toIdConverter", "Converter must not be null.");
        }

        public override string ConvertToString(object id)
        {
            if (!(id is T))
            {
                var msg = string.Format("Id is not of the type {0}.", this.IdType.FullName);
                throw new ArgumentException(msg, "id");
            }

            // Can throw exception, it's ok.
            return this.ToStringConverter.Invoke((T)id);
        }

        public override object ConvertFromString(string idString)
        {

            // Can throw exception, it's ok.
            return this.ToIdConverter.Invoke(idString);
        }
    }

    public static class BtbrdCoreIdConverters
    {
        private static ConcurrentDictionary<Type, BtbrdCoreIdConverter> Converters { get; } = new ConcurrentDictionary<Type, BtbrdCoreIdConverter>();

        public static void AddConverter(BtbrdCoreIdConverter converter)
        {
            if (!Converters.TryAdd(converter.IdType, converter))
            {
                BtbrdCoreIdConverter oldConverter = null;
                Converters.TryRemove(converter.IdType, out oldConverter);
                Converters.TryAdd(converter.IdType, converter);
            }
        }

        public static string ConvertToString(object obj)
        {
            if (obj == null) { return null; }
            if(Converters.TryGetValue(obj.GetType(), out var converter))
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
