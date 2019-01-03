using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.JsonApi.Attributes;
using Bitbird.Core.Json.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.JsonApi
{
    
    public class JsonApiResourceObject
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Include)]
        public string Type { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JsonApiRelationshipObjectBase> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }
    }

    //public static class JsonApiResourceObjectExtensions
    //{
    //    public static void SetIdAndType(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string customIdPropertyName = null, string customTypeName = null)
    //    {
    //        resourceObject.Type = (string.IsNullOrWhiteSpace(customTypeName)) ? customTypeName : data.GetJsonApiClassName();
            
    //        if (string.IsNullOrWhiteSpace(customIdPropertyName))
    //        {
    //            resourceObject.Id = data.GetIdAsString();
    //        }
    //        else
    //        {
    //            var dataType = data.GetType();
    //            var propertyInfo = dataType.GetProperty(customIdPropertyName);
    //            var value = propertyInfo.GetValueFast(data);
    //            resourceObject.Id = (value==null)?null:JValue.FromObject(value)?.ToString();
    //        }
            
    //    }

    //    public static void AddAttribute(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string propertyName, string attributeName = null)
    //    {
    //        var dataType = data.GetType();
    //        var propertyInfo = dataType.GetProperty(propertyName);
    //        var key = string.IsNullOrWhiteSpace(attributeName) ? StringUtils.GetAttributeName(propertyInfo) : attributeName;
    //        var value = propertyInfo.GetValueFast(data);
    //        if (resourceObject.Attributes == null) { resourceObject.Attributes = new Dictionary<string, object>(); }
    //        resourceObject.Attributes.Add(key, value);
    //    }

    //    public static void AddToOneRelationship(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string propertyName, string relationshipName = null)
    //    {
    //        var dataType = data.GetType();
    //        var propertyInfo = dataType.GetProperty(propertyName);
    //        var key = string.IsNullOrWhiteSpace(relationshipName) ? StringUtils.GetRelationShipName(propertyInfo) : relationshipName;
    //        var value = propertyInfo.GetValueFast(data) as IJsonApiDataModel;
    //        if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
    //        JsonApiToOneRelationshipObject relation = new JsonApiToOneRelationshipObject();
    //        if(value != null)
    //        {
    //            relation.Data = new JsonApiResourceIdentifierObject
    //            {
    //                Id = value.GetIdAsString(),
    //                Type = value.GetJsonApiClassName()
    //            };
    //        }

    //        resourceObject.Relationships.Add(key, relation);
    //    }

    //    public static void AddToManyRelationship(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string propertyName, string relationshipName = null)
    //    {
    //        var dataType = data.GetType();
    //        var propertyInfo = dataType.GetProperty(propertyName);
    //        var key = string.IsNullOrWhiteSpace(relationshipName) ? StringUtils.GetRelationShipName(propertyInfo) : relationshipName;
    //        var value = propertyInfo.GetValueFast(data) as IEnumerable<IJsonApiDataModel>;
    //        if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
    //        JsonApiToManyRelationshipObject relationshipCollection = new JsonApiToManyRelationshipObject();
    //        if (value != null)
    //        {
    //            foreach(var referencedData in value)
    //            {
    //                relationshipCollection.Data.Add( new JsonApiResourceIdentifierObject
    //                {
    //                    Id = referencedData.GetIdAsString(),
    //                    Type = referencedData.GetJsonApiClassName()
    //                });
    //            }
    //        }

    //        resourceObject.Relationships.Add(key, relationshipCollection);
    //    }
    //}


}
