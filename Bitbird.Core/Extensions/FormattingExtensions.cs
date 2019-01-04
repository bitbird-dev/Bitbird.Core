using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitbird.Core.Extensions
{
    public static class FormattingExtensions
    {
        public static string SequenceToString<T>(this IEnumerable<T> collection, string delimiter = null, Func<T, int, string> elementFormatter = null, Func<string, string> formattedSequenceEmbedder = null, bool formattedSequenceEmbedderForContentOnly = false)
        {
            delimiter = delimiter ?? "; ";
            formattedSequenceEmbedder = formattedSequenceEmbedder ?? (content => content);
            var formattedSequenceEmbedderForNoContent = !formattedSequenceEmbedderForContentOnly ? formattedSequenceEmbedder : (content => content);

            if (collection == null)
                return formattedSequenceEmbedderForNoContent("null");

            var collectionEnumerated = collection.ToArray();

            if (collectionEnumerated.Length == 0)
                return formattedSequenceEmbedderForNoContent(string.Empty);

            elementFormatter = elementFormatter ?? ((entry, i) => entry == null ? "null" : Convert.ToString(entry));
            
            var sb = new StringBuilder();
            var idx = 0;
            foreach (var entry in collectionEnumerated)
            {
                if (idx != 0)
                    sb.Append(delimiter);

                sb.Append(elementFormatter(entry, idx));
                idx++;
            }

            return formattedSequenceEmbedder(sb.ToString());
        }

        public static string ConsistentNewLines(this string str, string newLine = null)
        {
            return str?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", newLine ?? Environment.NewLine);
        }
    }
}