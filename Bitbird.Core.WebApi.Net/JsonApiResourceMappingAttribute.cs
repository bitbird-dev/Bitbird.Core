using System;

namespace Bitbird.Core.WebApi.Net
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonApiResourceMappingAttribute : Attribute
    {
        public readonly Type Type;

        public JsonApiResourceMappingAttribute(Type type)
        {
            Type = type;
        }
    }
}