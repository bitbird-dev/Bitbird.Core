using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// Stores documentation information about one member.
    /// </summary>
    public class MemberDocumentation
    {
        /// <summary>
        /// The member name.
        /// </summary>
        public readonly string MemberName;

        /// <summary>
        /// Stores the documentation content by the content type.
        /// Content types can i.e. be <c>"summary"</c>.
        /// </summary>
        private readonly Dictionary<string, string> contentByType;

        /// <summary>
        /// Constructs a <see cref="MemberDocumentation"/>.
        /// See the class documentation of <see cref="MemberDocumentation"/>.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <param name="contentByType">Stores the documentation content by the content type.</param>
        public MemberDocumentation(string memberName, Dictionary<string, string> contentByType)
        {
            MemberName = memberName;
            this.contentByType = contentByType;
        }


        private static readonly Regex RegexSeeCref = new Regex("[<]see\\s+cref\\s*[=]\\s*[\"](?<id>[-+{}:.`a-zA-Z0-9]+)[\"]\\s*[/][>]");

        /// <summary>
        /// Returns the passed documentation type for this member or null if it is not defined.
        /// </summary>
        /// <param name="documentationType">The documentation type.</param>
        /// <param name="documentationCollection">A documentation collection to resolve see-tags.</param>
        /// <returns>The documentation as string or null if not found.</returns>
        public string Get(DocumentationType documentationType, DocumentationCollection documentationCollection)
        {
            if (!contentByType.TryGetValue(documentationType.ToString().ToLower(), out var doc))
                return null;
            if (documentationCollection == null)
                return doc;

            var referenced = new Dictionary<string, string>();

            string Resolve(Func<string> getContent, Action<string> setContent)
            {
                var value = getContent();
                var found = new List<string>();
                
                while (true)
                {
                    var match = RegexSeeCref.Match(value);

                    if (!match.Success)
                        break;

                    var referencedId = match.Groups["id"].Value;

                    if (!referenced.TryGetValue(referencedId, out var referencedDoc))
                    {
                        referencedDoc = documentationCollection.ForName(referencedId)?.Get(DocumentationType.Summary, null) ?? string.Empty;
                        referenced[referencedId] = referencedDoc;
                        found.Add(referencedId);
                    }

                    value = value.Replace(match.Value, $"Reference({referencedId})");
                }

                setContent(value);

                foreach (var item in found)
                    Resolve(() => referenced[item], content => referenced[item] = content);

                return value;
            }

            var sb = new StringBuilder();

            sb.Append(Resolve(() => doc, content => { }));

            foreach (var reference in referenced)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"START Reference({reference.Key})");
                sb.AppendLine($"{reference.Value}");
                sb.AppendLine($"END Reference({reference.Key})");
            }

            return sb.ToString();
        }
    }
}