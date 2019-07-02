using System;

namespace Bitbird.Core.Web.JsonApi
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