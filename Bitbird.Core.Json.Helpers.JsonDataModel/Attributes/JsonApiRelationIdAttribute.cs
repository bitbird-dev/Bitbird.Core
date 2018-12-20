using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.Helpers.JsonDataModel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JsonApiRelationIdAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public JsonApiRelationIdAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
