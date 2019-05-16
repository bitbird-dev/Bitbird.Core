using System.Linq;
using System.Text;

namespace Bitbird.Core
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var c in s)
            {
                if (first && char.IsLetter(c))
                {
                    sb.Append(char.ToLower(c));
                    first = false;
                    continue;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
        public static string ToKebabCase(this string s)
        {
            return new string(s.SelectMany(c => char.IsUpper(c) ? new[] { '-', char.ToLower(c) } : new[] { c }).ToArray()).Trim('-');
        }
        public static string FromKebabCase(this string s)
        {
            var sb = new StringBuilder();
            var upper = false;
            var first = true;
            foreach (var c in s)
            {
                if (first)
                {
                    sb.Append(char.ToUpper(c));
                    first = false;
                    continue;
                }

                if (c == '-')
                {
                    upper = true;
                    continue;
                }


                if (upper)
                {
                    sb.Append(char.ToUpper(c));
                    upper = false;
                    continue;
                }
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}