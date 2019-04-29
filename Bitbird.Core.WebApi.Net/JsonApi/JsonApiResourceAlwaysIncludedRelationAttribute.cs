using System;

namespace Bitbird.Core.WebApi.Net.JsonApi
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class JsonApiResourceAlwaysIncludedRelationAttribute : Attribute
    {
        public readonly string IdRelationPropertyName;

        public JsonApiResourceAlwaysIncludedRelationAttribute(string idRelationPropertyName)
        {
            IdRelationPropertyName = idRelationPropertyName;
        }
    }
}