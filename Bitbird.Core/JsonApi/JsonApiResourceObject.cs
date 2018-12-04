using Bitbird.Core.Extensions;
using Bitbird.Core.JsonApi.Attributes;
using Bitbird.Core.JsonApi.UrlBuilder;
using Bitbird.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    /// <summary>
    /// A resource object MUST contain at least the following top-level members:
    /// 
    ///     id
    ///     type
    /// 
    /// Exception: The id member is not required when the resource object originates at the client and represents a new resource to be created on the server.
    /// 
    /// In addition, a resource object MAY contain any of these top-level members:
    /// 
    ///     attributes: an attributes object representing some of the resource’s data.
    ///     relationships: a relationships object describing relationships between the resource and other JSON API resources.
    ///     links: a links object containing links related to the resource.
    ///     meta: a meta object containing non-standard meta-information about a resource that can not be represented as an attribute or relationship.
    /// 
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class JsonApiResourceObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        public string Type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JObject Attributes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JsonApiRelationshipBase> Relationships { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        public JsonApiResourceObject()
        {

        }

        /// <summary>
        /// Creates a new JsonApiResourceObject. 
        /// Id, Type, Attribute and Relationships properties are automatically set based on the supplied data instance. 
        /// Note that depending on the data instance passed to the constructor these fields might be null.
        /// </summary>
        /// <param name="data">must not be null!</param>
        /// <param name="processRelationships">if set to false, the Relationships property will not be generated and stay null</param>
        public JsonApiResourceObject(JsonApiBaseModel data, Uri queryUri = null, bool processRelationships = true)
        {
            Type type = data.GetType();
            Id = data.Id;
            Type = StringUtils.ToTrimmedLowerCase(type.Name);

            // set url
            if(queryUri != null)
            {
                Links = new JsonApiLinksObject { Self = new JsonApiLink( queryUri.GetFullHost() + (new DefaultUrlPathBuilder()).BuildCanonicalPath(data)) };
            }  

            // extract attributes and relations
            Attributes = new JObject();
            Relationships = new Dictionary<string, JsonApiRelationshipBase>();

            var propertiesArray = type.GetProperties();
            foreach (var propertyInfo in propertiesArray)
            {
                // check for existing ignore attributes
                var ignoreAttribute = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                if (ignoreAttribute != null) { continue; }

                // check for existing ignore attributes
                var accessAttribute = propertyInfo.GetCustomAttribute<JsonAccessRestrictedAttribute>();
                if (accessAttribute != null && !data.IsPropertyAccessible(propertyInfo))
                {
                    continue;
                }

                ExtractAttributeAndRelationsFromProperty(Attributes, propertyInfo, data, processRelationships);
            }
            if (Attributes.Count < 1) { Attributes = null; }
            if (Relationships.Count < 1) { Relationships = null; }
        }

        private void ExtractAttributeAndRelationsFromProperty(JObject targetNode, PropertyInfo propertyInfo, JsonApiBaseModel data, bool processRelationships)
{
            Type propertyType = propertyInfo.PropertyType;

            // Directly add primitive as well as string properties to the Attributes
            if (propertyType.IsPrimitive || propertyType.IsValueType || propertyType == typeof(string))
            {
                targetNode.Add(new JProperty(StringUtils.ToCamelCase(propertyInfo.Name), propertyInfo.GetValue(data)));
            }
            else if (propertyType.IsNonStringEnumerable())
            {
                var innerType = propertyType.GenericTypeArguments?[0];
                if (innerType == null) return;

                var enumeratedData = propertyInfo.GetValue(data) as IEnumerable;
                
                // if inner type is Primitve or string
                if (innerType.IsPrimitive || innerType.IsValueType || innerType == typeof(string))
                {
                    JArray array = new JArray();
                    foreach(var element in enumeratedData)
                    {
                        array.Add(new JValue(element));
                    }
                    targetNode.Add(new JProperty(StringUtils.ToCamelCase(propertyInfo.Name), array));
                }
                else if (processRelationships && innerType.IsSubclassOf(typeof(JsonApiBaseModel)))
                {
                    var relations = (enumeratedData as IEnumerable<JsonApiBaseModel>).Select(
                        x => new JsonApiResourceIdentifierObject(x.Id, StringUtils.ToTrimmedLowerCase(innerType.Name)));
                    Relationships.Add(StringUtils.ToSnakeCase(propertyInfo.Name), new JsonApiToManyRelationship{Data = relations});
                }
            }
            // Classes derived from JsonApiBaseModel will be added to the relationships collection.
            else if (processRelationships && propertyType.IsSubclassOf(typeof(JsonApiBaseModel)))
            {
                // add relationship
                var value = propertyInfo.GetValue(data) as JsonApiBaseModel;
                if (value != null) { AddJsonApiToOneRelationship(propertyInfo, propertyType, value); }
            }
        }
        
        /// <summary>
        /// Adds a new JsonApiToOneRelationship to the Relationships rroperty.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="propertyType"></param>
        /// <param name="rawdata"></param>
        private void AddJsonApiToOneRelationship(PropertyInfo propertyInfo, Type propertyType, JsonApiBaseModel rawdata)
        {
            Relationships.Add(StringUtils.ToSnakeCase(propertyInfo.Name), new JsonApiToOneRelationship
            {
                Data = new JsonApiResourceIdentifierObject(rawdata.Id, StringUtils.ToTrimmedLowerCase(propertyType.Name))
            });
        }
    }
}
