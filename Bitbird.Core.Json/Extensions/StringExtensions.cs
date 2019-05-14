using Bitbird.Core.Json.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static string FromCamelCaseToJsonCamelCase(this string expression)
        {
            return new string(expression.Select((c, idx) => idx == 0 || expression[idx-1] == '/' ? char.ToLower(c) : c).ToArray());
        }
    }
}
