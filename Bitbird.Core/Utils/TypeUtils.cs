using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Utils
{
    public static class TypeUtils
    {
        public static bool IsNonStringEnumerable(this Type type)
        {
            return type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type);
        }
    }
}
