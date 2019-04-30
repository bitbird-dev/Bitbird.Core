using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// Provides access to structured code documentation from projects (if XML-documentation-export is supported).
    /// </summary>
    public class DocumentationCollection
    {
        /// <summary>
        /// All member documentation (types, properties, ect) by their documentation names.
        /// </summary>
        private readonly Dictionary<string, MemberDocumentation> membersByName;

        /// <summary>
        /// Constructs a <see cref="DocumentationCollection"/>.
        ///
        /// Loads all XML-files from a directory (non-recursive) and scans them for XML-code-documentation.
        /// </summary>
        /// <param name="baseDirectory">The directory to scan.</param>
        public DocumentationCollection(string baseDirectory)
        {
            membersByName = Directory.EnumerateFiles(baseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                .Select(XDocument.Load)
                .SelectMany(doc => doc.Root?.Elements("members").Elements("member"))
                .Select(member =>
                {
                    var tags = member.Elements()
                        .Select(element => new
                        {
                            Tag = element.Name.ToString().ToLower(),
                            Name = (string) element.Attribute("name"),
                            InnerXml = GetInnerXml(element)
                        })
                        .Select(x => new
                        {
                            Id = $"{x.Tag}{(x.Name == null ? string.Empty : ".")}{x.Name}",
                            Doc = x.InnerXml
                        })
                        .ToArray();

                    return new MemberDocumentation(
                        (string) member.Attribute("name"), 
                        tags
                            .GroupBy(tag => tag.Id)
                            .ToDictionary(x => x.Key, x => x.First().Doc));
                })
                .GroupBy(memberDoc => memberDoc.MemberName)
                .ToDictionary(memberDoc => memberDoc.Key, memberDoc => memberDoc.First());
        }

        /// <summary>
        /// Returns a <see cref="MemberDocumentation"/> for a given member name, or null if no documentation was found.
        /// </summary>
        /// <param name="name">The member name.</param>
        /// <returns>The documentation.</returns>
        public MemberDocumentation ForName(string name)
        {
            return membersByName.TryGetValue(name, out var doc) ? doc : null;
        }

        /// <summary>
        /// Returns a <see cref="MemberDocumentation"/> for a type, or null if no documentation was found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The documentation.</returns>
        public MemberDocumentation ForClass(Type type) => ForName($"T:{type.Namespace}.{type.Name}");

        /// <summary>
        /// Returns a <see cref="MemberDocumentation"/> for a method, or null if no documentation was found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">The method name.</param>
        /// <returns>The documentation.</returns>
        public MemberDocumentation ForMethod(Type type, string methodName) => ForName($"P:{type.Namespace}.{type.Name}.{methodName}");

        /// <summary>
        /// Returns a <see cref="MemberDocumentation"/> for a property, or null if no documentation was found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The documentation.</returns>
        public MemberDocumentation ForProperty(Type type, string propertyName) => ForName($"P:{type.Namespace}.{type.Name}.{propertyName}");

        /// <summary>
        /// Returns a <see cref="MemberDocumentation"/> for a field, or null if no documentation was found.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldName">The field name.</param>
        /// <returns>The documentation.</returns>
        public MemberDocumentation ForField(Type type, string fieldName) => ForName($"F:{type.Namespace}.{type.Name}.{fieldName}");

        /// <summary>
        /// Returns the inner xml of an xml-node as string.
        /// </summary>
        /// <param name="element">The element of which to return the inner xml.</param>
        /// <returns>The inner xml as string.</returns>
        private static string GetInnerXml(XContainer element)
        {
            if (element == null)
                return string.Empty;

            var innerXml = new StringBuilder();

            // append node's xml string to innerXml
            foreach (var node in element.Nodes())
                innerXml.Append(node);

            return innerXml.ToString();
        }
    }
}