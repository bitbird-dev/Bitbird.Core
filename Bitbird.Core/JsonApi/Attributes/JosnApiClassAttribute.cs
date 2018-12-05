using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JsonApiClassAttribute : Attribute
    {

        public JsonApiClassAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
