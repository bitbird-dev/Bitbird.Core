using Bitbird.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class JsonApiResourceObject
    {

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

        public JsonApiResourceObject(JsonApiBaseModel data, bool processRelationships = true)
        {

            Type type = data.GetType();
            Id = data.Id;
            Type = StringUtils.ToTrimmedLowerCase(type.Name);

            // extract attributes and relations
            Attributes = new JObject();
            Relationships = new Dictionary<string, JsonApiRelationshipBase>();

            var propertiesArray = type.GetProperties();
            foreach (var propertyInfo in propertiesArray)
            {
                if (propertyInfo.Name == nameof(JsonApiBaseModel.Id)) { continue; }
                ProcessProperty(Attributes, propertyInfo, data, processRelationships);
            }
            if (Attributes.Count < 1) { Attributes = null; }
            if (Relationships.Count < 1) { Relationships = null; }
        }



        private void ProcessProperty(JObject targetNode, PropertyInfo propertyInfo, JsonApiBaseModel data, bool processRelationships)
{
            Type propertyType = propertyInfo.PropertyType;

            if (propertyType.IsPrimitive || propertyType.IsValueType || propertyType == typeof(string))
            {
                targetNode.Add(new JProperty(propertyInfo.Name, propertyInfo.GetValue(data)));
            }
            else if (propertyType.IsNonStringEnumerable())
            {
                var innerType = propertyType.GenericTypeArguments?[0];
                if(innerType == null) { throw new NotSupportedException("Enumerable "+ propertyType.Name + " does not contain a generic type."); }

                var enumeratedData = propertyInfo.GetValue(data) as IEnumerable;
                
                // if inner type is Primitve or string
                if (innerType.IsPrimitive || innerType.IsValueType || innerType == typeof(string))
                {
                    JArray array = new JArray();
                    foreach(var element in enumeratedData)
                    {
                        array.Add(new JValue(element));
                    }
                    targetNode.Add(new JProperty(propertyInfo.Name, array));
                }
                else if (processRelationships && innerType.IsSubclassOf(typeof(JsonApiBaseModel)))
                {
                    IEnumerable<JsonApiResourceIdentifierObject> relations = (enumeratedData as IEnumerable<JsonApiBaseModel>).Select(
                        x => new JsonApiResourceIdentifierObject(x.Id, StringUtils.ToTrimmedLowerCase(innerType.Name)));
                    Relationships.Add(StringUtils.ToSnakeCase(propertyInfo.Name), new JsonApiToManyRelationship{Data = relations});
                }
            }
            else if (processRelationships && propertyType.IsSubclassOf(typeof(JsonApiBaseModel)))
            {
                // add relationship
                AddRelationship(propertyInfo, propertyType, propertyInfo.GetValue(data) as JsonApiBaseModel);
            }
        }

        private void AddRelationship(PropertyInfo propertyInfo, Type propertyType, JsonApiBaseModel rawdata)
        {
            Relationships.Add(StringUtils.ToSnakeCase(propertyInfo.Name), new JsonApiToOneRelationship
            {
                Data = new JsonApiResourceIdentifierObject(rawdata.Id, StringUtils.ToTrimmedLowerCase(propertyType.Name))
            });
        }
    }
}
