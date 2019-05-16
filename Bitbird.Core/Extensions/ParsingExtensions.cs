using System;

namespace Bitbird.Core
{
    public static class ParsingExtensions
    {
        public static object ParseAs(this string value, Type t)
        {
            if (t == typeof(string))
                return value;

            if (string.IsNullOrEmpty(value) && t.IsClass)
                return null;

            if (value == null)
                throw new Exception($"Cannot convert null to {t.FullName}.");

            var baseNullable = Nullable.GetUnderlyingType(t) ?? t;

            object result;

            if (baseNullable.IsEnum)
                result = Enum.ToObject(baseNullable, Convert.ChangeType(value, Enum.GetUnderlyingType(baseNullable)));
            else
                result = Convert.ChangeType(value, baseNullable);


            if (baseNullable == typeof(DateTime))
                result = ((DateTime)result).ToUniversalTime();

            return result;
        }
        public static T ParseAs<T>(this string value)
        {
            return (T)ParseAs(value, typeof(T));
        }
    }
}
