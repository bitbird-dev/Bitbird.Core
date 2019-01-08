using System.Text.RegularExpressions;

namespace Bitbird.Core.Extensions
{
    public static class WildCardExtensions
    {
        public static string WildCardPatternToRegexPattern(this string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}