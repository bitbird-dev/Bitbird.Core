using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbird.Core.Types
{
    public static class TypeFormatter
    {
        public static string ToCsType(this Type type, Action<Type> foundType = null)
        {
            string FormatPrimitiveType(Type t)
            {
                var underlying = Nullable.GetUnderlyingType(t) ?? t;
                var nullable = underlying != t;

                string FormatNullable(string name) => nullable ? $"{name}?" : name;

                if (underlying == typeof(string))
                    return "string";
                if (underlying == typeof(char))
                    return FormatNullable("char");
                if (underlying == typeof(bool))
                    return FormatNullable("bool");
                if (underlying == typeof(long))
                    return FormatNullable("long");
                if (underlying == typeof(ulong))
                    return FormatNullable("ulong");
                if (underlying == typeof(uint))
                    return FormatNullable("uint");
                if (underlying == typeof(int))
                    return FormatNullable("int");
                if (underlying == typeof(ushort))
                    return FormatNullable("ushort");
                if (underlying == typeof(short))
                    return FormatNullable("short");
                if (underlying == typeof(byte))
                    return FormatNullable("byte");
                if (underlying == typeof(sbyte))
                    return FormatNullable("sbyte");
                if (underlying == typeof(float))
                    return FormatNullable("float");
                if (underlying == typeof(double))
                    return FormatNullable("double");
                if (underlying == typeof(decimal))
                    return FormatNullable("decimal");
                if (underlying == typeof(Guid))
                    return FormatNullable("Guid");
                if (underlying == typeof(DateTime))
                    return FormatNullable("DateTime");
                if (underlying == typeof(TimeSpan))
                    return FormatNullable("TimeSpan");
                if (underlying == typeof(DateTimeOffset))
                    return FormatNullable("DateTimeOffset");

                if (underlying.IsArray)
                    return $"{FormatNullable(FormatPrimitiveType(underlying.GetElementType()))}[]";

                var iEnumerable = underlying.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (iEnumerable != null)
                    return $"{FormatNullable(FormatPrimitiveType(iEnumerable.GetGenericArguments()[0]))}[]";

                foundType?.Invoke(underlying);

                return FormatNullable(underlying.Name);
            }

            return FormatPrimitiveType(type);
        }
        public static string ToJsType(this Type type, Action<Type> foundType = null)
        {
            string FormatPrimitiveType(Type t)
            {
                var underlying = Nullable.GetUnderlyingType(t) ?? t;
                var nullable = underlying != t;

                string FormatNullable(string name) => nullable ? $"{name}?" : name;

                if (underlying == typeof(string))
                    return "string";
                if (underlying == typeof(char))
                    return "number";
                if (underlying == typeof(bool))
                    return "boolean";
                if (underlying == typeof(long))
                    return "number";
                if (underlying == typeof(ulong))
                    return "number";
                if (underlying == typeof(uint))
                    return "number";
                if (underlying == typeof(int))
                    return "number";
                if (underlying == typeof(ushort))
                    return "number";
                if (underlying == typeof(short))
                    return "number";
                if (underlying == typeof(byte))
                    return "number";
                if (underlying == typeof(sbyte))
                    return "number";
                if (underlying == typeof(float))
                    return "number";
                if (underlying == typeof(double))
                    return "number";
                if (underlying == typeof(decimal))
                    return "number";
                if (underlying == typeof(Guid))
                    return "string";
                if (underlying == typeof(DateTime))
                    return "date";
                if (underlying == typeof(TimeSpan))
                    return "date";
                if (underlying == typeof(DateTimeOffset))
                    return "date";
                if (underlying.IsEnum)
                {
                    foundType(underlying);
                    return "number";
                }

                if (underlying.IsArray)
                    return $"{FormatNullable(FormatPrimitiveType(underlying.GetElementType()))}[]";

                var iEnumerable = underlying.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (iEnumerable != null)
                    return $"{FormatNullable(FormatPrimitiveType(iEnumerable.GetGenericArguments()[0]))}[]";

                foundType?.Invoke(underlying);

                return FormatNullable(underlying.Name);
            }

            return FormatPrimitiveType(type);
        }
    }
}