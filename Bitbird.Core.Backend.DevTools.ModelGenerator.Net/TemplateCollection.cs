using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// Stores all templates for a target format.
    /// </summary>
    public class TemplateCollection
    {
        private readonly Dictionary<TemplateType, string> templates;

        /// <summary>
        /// Constructs a <see cref="TemplateCollection"/>.
        /// </summary>
        /// <param name="targetFormat">The target format. Used as prefix for resources to load.</param>
        public TemplateCollection(string targetFormat, ResourceManager resourceManager)
        {
            templates = Enum.GetValues(typeof(TemplateType))
                .Cast<TemplateType>()
                .Select(template =>
                {
                    var name = $"{targetFormat}{template}";

                    try
                    {
                        return new
                        {
                            Template = template,
                            Content = resourceManager.GetString(name) ?? throw new NullReferenceException()
                        };
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Could not find the templateType {template} for the target format {targetFormat} (Details: {exception}).");
                    }
                })
                .ToDictionary(t => t.Template, t => t.Content);
        }

        /// <summary>
        /// Returns the content of a specific templateType for the current target format.
        /// </summary>
        /// <param name="templateType">The templateType to return.</param>
        /// <returns>The content of the templateType.</returns>
        public string Get(TemplateType templateType)
        {
            return templates[templateType];
        }
    }
}