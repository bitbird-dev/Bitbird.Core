using System;

namespace Bitbird.Core.Extensions
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

            var baseNullable = Nullable.GetUnderlyingType(t);
            if (baseNullable != null)
                return Convert.ChangeType(value, baseNullable);

            return Convert.ChangeType(value, t);
        }
        public static T ParseAs<T>(this string value)
        {
            return (T)ParseAs(value, typeof(T));
        }
    }
}
