using System.Text.RegularExpressions;
using CommonMark;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Helper
{
    public static class MarkupHelper
    {
        private static readonly Regex regexHtml = new Regex("(\\<script(.+?))|(\\<style(.+?))|(<.*?>)|(^\\s+$[\\r\\n]*)|(\\r)",
            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string GetSingleLinePlainTextFromHtml([CanBeNull] string htmlString)
        {
            if (htmlString == null)
                return null;

            htmlString = regexHtml.Replace(htmlString, string.Empty);
            htmlString = htmlString.Replace("\n", " ");
            htmlString = htmlString.Replace("  ", " ");

            return htmlString;
        }

        public static string GetSingleLinePlainTextFromMarkdown([CanBeNull] string mdString)
        {
            if (mdString == null)
                return null;

            var htmlString = CommonMarkConverter.Convert(mdString);

            return GetSingleLinePlainTextFromHtml(htmlString);
        }
    }
}
