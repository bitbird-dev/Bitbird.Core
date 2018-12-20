using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
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
            var dataType = data.GetType();
            var propertyInfo = dataType.GetProperty(relationship.PropertyName);
            var key = relationship.Name;
            var value = propertyInfo.GetValueFast(data);
            if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
            JsonApiToOneRelationshipObject relation = new JsonApiToOneRelationshipObject();
            if (value != null)
            {
                relation.Data = new JsonApiResourceIdentifierObject
                {
                    Id = GetApiResourceId(value, relationship.RelatedResource)?.ToString(),
                    Type = relationship.RelatedResource.ResourceType
                };
            }

            resourceObject.Relationships.Add(key, relation);
        }

        #endregion

        #region AddToManyRelationship

        internal static void AddToManyRelationship(this JsonApiResourceObject resourceObject, object data, ResourceRelationship relationship)
        {
            var dataType = data.GetType();
            var propertyInfo = dataType.GetProperty(relationship.PropertyName);
            var key = relationship.Name;
            var value = propertyInfo.GetValueFast(data) as IEnumerable<object>;
            if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
            JsonApiToManyRelationshipObject relationshipCollection = new JsonApiToManyRelationshipObject();
            if (value != null)
            {
                foreach (var referencedData in value)
                {
                    relationshipCollection.Data.Add(new JsonApiResourceIdentifierObject
                    {
                        Id = GetApiResourceId(referencedData, relationship.RelatedResource)?.ToString(),
                        Type = relationship.RelatedResource.ResourceType
                    });
                }
            }

            resourceObject.Relationships.Add(key, relationshipCollection);
        }

        #endregion
    }
}
