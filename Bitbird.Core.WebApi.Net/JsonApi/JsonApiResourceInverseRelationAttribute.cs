using System;
using Bitbird.Core.Json.Helpers.ApiResource;

namespace Bitbird.Core.WebApi.JsonApi
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JsonApiResourceInverseRelationAttribute : Attribute
    {
        public readonly string LocalAttribute;
        public readonly Type RelatedResource;
        public readonly string RelatedAttribute;

        public JsonApiResourceInverseRelationAttribute(string localAttribute, Type relatedResource, string relatedAttribute)
        {
            if (!typeof(JsonApiResource).IsAssignableFrom(relatedResource))
                throw new ArgumentException(nameof(relatedResource), $"An inverse relation was defined but the passed {nameof(relatedResource)} is not a {nameof(JsonApiResource)} (Passed: {relatedResource.FullName}).");

            LocalAttribute = localAttribute;
            RelatedResource = relatedResource;
            RelatedAttribute = relatedAttribute;
        }
    }
}