using Bitbird.Core.Json.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Extensions
{
    public static class StringExtensions
    {
        public static string ToJsonAttributeName(this string inputstring)
        {
            return StringUtils.ToCamelCase(inputstring);
        }

        public static string ToJsonRelationshipName(this string inputstring)
        {
            return StringUtils.ToSnakeCase(inputstring);
        }
    }
}
