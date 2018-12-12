using Bitbird.Core.JsonApi.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Utils
{
    public static class StringUtils
    {
        #region Extensions

        public static string TrimJoin(this char separator, params string[] parts)
        {
            return string.Join(separator.ToString(), parts.Select(p => p.Trim(separator)));
        }

        public static string EnsureEndsWith(this string source, string end)
        {
            return source.EndsWith(end)
                ? source
                : source + end;
        }

        public static string EnsureStartsWith(this string source, string start)
        {
            return source.StartsWith(start)
                ? source
                : start + source;
        }

        #endregion

        #region CamelCase

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    if (char.IsSeparator(chars[i + 1]))
                    {
                        
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }

        #endregion

        #region SnakeCase

        public static string ToSnakeCase(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            StringBuilder sb = new StringBuilder();
            DasherizedState state = DasherizedState.Start;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != DasherizedState.Start)
                    {
                        state = DasherizedState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case DasherizedState.Upper:
                            bool hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext)
                            {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != '-')
                                {
                                    sb.Append('-');
                                }
                            }
                            break;
                        case DasherizedState.Lower:
                        case DasherizedState.NewWord:
                            sb.Append('-');
                            break;
                    }

                    char c = char.ToLowerInvariant(s[i]);
                    sb.Append(c);

                    state = DasherizedState.Upper;
                }
                else if (s[i] == '-')
                {
                    sb.Append('-');
                    state = DasherizedState.Start;
                }
                else
                {
                    if (state == DasherizedState.NewWord)
                    {
                        sb.Append('-');
                    }

                    sb.Append(s[i]);
                    state = DasherizedState.Lower;
                }
            }

            return sb.ToString();
        }

        internal enum DasherizedState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        #endregion

        #region PluralizedLowerCase

        public static string ToTrimmedLowerCase(string s)
        {
            string result = s.Trim();
            return result.ToLowerInvariant();
        }

        #endregion

        public static string GetRelationShipName(PropertyInfo propertyInfo)
        {
            return ToSnakeCase(GetAttributeName(propertyInfo));
        }

        public static string GetAttributeName(PropertyInfo propertyInfo)
        {
            var customName = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            string attributeName = (customName != null) ? customName.PropertyName : propertyInfo.Name;
            return attributeName = attributeName.Trim();
        }
    }
}
