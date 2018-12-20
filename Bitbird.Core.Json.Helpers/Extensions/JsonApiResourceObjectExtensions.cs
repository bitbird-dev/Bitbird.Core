using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Json.Helpers.JsonDataModel.Converters;
using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    public static class JsonApiResourceObjectExtensions
    {
        internal static void FromApiResource(this JsonApiResourceObject resourceObject, object data, JsonApiResource apiResource)
        {
            resourceObject.SetIdAndType(data, apiResource);
            foreach (var attr in apiResource.Attributes)
            {
                resourceObject.AddAttribute(data, attr);
            }
            foreach (var realtionship in apiResource.Relationships)
            {
                if (realtionship.Kind == RelationshipKind.BelongsTo)
                {
                    resourceObject.AddToOneRelationship(data, realtionship);
                }
                else
                {
                    resourceObject.AddToManyRelationship(data, realtionship);
                }
            }
        }


        #region SetIdAndType

        internal static void SetIdAndType(this JsonApiResourceObject resourceObject, object data, JsonApiResource apiResource)
        {
            resourceObject.Type = apiResource.ResourceType;
            resourceObject.Id = GetApiResourceId(data, apiResource)?.ToString();
        }

        #endregion

        #region GetApiResourceId

        internal static object GetApiResourceId(object data, JsonApiResource apiResource)
        {
            var propertyInfo = data.GetType().GetProperty(apiResource.IdProperty);
            return propertyInfo.GetValueFast(data);
        }

        #endregion

        #region AddAttribute

        internal static void AddAttribute(this JsonApiResourceObject resourceObject, object data, ResourceAttribute resource)
        {
            var dataType = data.GetType();
            var propertyInfo = dataType.GetProperty(resource.InternalName);
            var key = resource.Name;
            var value = propertyInfo.GetValueFast(data);
            if (resourceObject.Attributes == null) { resourceObject.Attributes = new Dictionary<string, object>(); }
            resourceObject.Attributes.Add(key, value);
        }

        #endregion

        #region AddToOneRelationship

        internal static void AddToOneRelationship(this JsonApiResourceObject resourceObject, object data, ResourceRelationship relationship)
        {
            string id = null;
            if (string.IsNullOrWhiteSpace(relationship.IdPropertyName))
            {
                var propertyInfo = data.GetType().GetProperty(relationship.PropertyName);
                var value = propertyInfo.GetValueFast(data);
                id = BtbrdCoreIdConverters.ConvertToString(GetApiResourceId(value, relationship.RelatedResource));
            }
            else
            {
                var idPropertyInfo = data.GetType().GetProperty(relationship.IdPropertyName);
                id = BtbrdCoreIdConverters.ConvertToString(idPropertyInfo?.GetValueFast(data));
            }

            if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
            JsonApiToOneRelationshipObject relation = new JsonApiToOneRelationshipObject();
            if (id != null)
            {
                relation.Data = new JsonApiResourceIdentifierObject
                {
                    Id = id,
                    Type = relationship.RelatedResource.ResourceType
                };
            }

            resourceObject.Relationships.Add(relationship.Name, relation);
        }

        #endregion

        #region AddToManyRelationship

        internal static void AddToManyRelationship(this JsonApiResourceObject resourceObject, object data, ResourceRelationship relationship)
        {
            IEnumerable<string> ids = null;
            if (string.IsNullOrWhiteSpace(relationship.IdPropertyName))
            {
                var propertyInfo = data.GetType().GetProperty(relationship.PropertyName);
                var values = propertyInfo.GetValueFast(data) as IEnumerable<object>;
                ids = values?.Select(x=> BtbrdCoreIdConverters.ConvertToString(GetApiResourceId(x, relationship.RelatedResource)));
            }
            else
            {
                var idPropertyInfo = data.GetType().GetProperty(relationship.IdPropertyName);

                var values = idPropertyInfo?.GetValueFast(data) as IEnumerable;
                var list = new List<string>();
                foreach(var id in values) { list.Add(BtbrdCoreIdConverters.ConvertToString(id)); }
                ids = list.AsEnumerable();
            }
            
            if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
            JsonApiToManyRelationshipObject relationshipCollection = new JsonApiToManyRelationshipObject();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    relationshipCollection.Data.Add(new JsonApiResourceIdentifierObject
                    {
                        Id = id,
                        Type = relationship.RelatedResource.ResourceType
                    });
                }
            }

            resourceObject.Relationships.Add(relationship.Name, relationshipCollection);
        }

        #endregion
    }
}
