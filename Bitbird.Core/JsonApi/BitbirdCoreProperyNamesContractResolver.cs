using Bitbird.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public class BitbirdCorePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver, IBitbirdCoreJsonApiContractResolver
    {

        public string ResolveRelationshipName(string propertyName)
        {
            return StringUtils.ToSnakeCase(propertyName);
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            return StringUtils.ToCamelCase(propertyName);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (typeof(JsonApiRelationshipBase).IsAssignableFrom(prop.PropertyType))
            {
                prop.PropertyName = ResolveRelationshipName(prop.PropertyName);
            }
            else
            {
                prop.PropertyName = ResolvePropertyName(prop.PropertyName);
            }
            return prop;
        }
    }
}
