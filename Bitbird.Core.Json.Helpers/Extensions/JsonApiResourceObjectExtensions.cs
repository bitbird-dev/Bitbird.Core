using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.Base.Converters;
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

        #region ToObject

        public static T_Result ToObject<T_Result, T_Resource>(this JsonApiResourceObject resourceObject) where T_Resource : JsonApiResource
        {
            return (T_Result)resourceObject.ToObject(Activator.CreateInstance<T_Resource>(), typeof(T_Result));
        }

        /// <summary>
        /// Attempts to instatiate an object of type <paramref name="targetType"/> with data from <paramref name="resourceObject"/> using the <paramref name="apiResource"/>.
        /// </summary>
        /// <param name="resourceObject"></param>
        /// <param name="apiResource"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ToObject(this JsonApiResourceObject resourceObject, JsonApiResource apiResource, Type targetType)
        {
            var result = Activator.CreateInstance(targetType);

            // extract id
            if (!string.IsNullOrWhiteSpace(resourceObject.Id))
            {
                var idProp = targetType.GetProperty(apiResource.IdProperty);
                if (idProp != null)
                {
                    var idObject = BtbrdCoreIdConverters.ConvertFromString(resourceObject.Id, idProp.PropertyType);
                    idProp.SetValueFast(result, idObject);
                }
            }

#warning "better iterate over attributes and relations defined in the apiresource"

            // extract attributes
            if(resourceObject.Attributes != null)
            {
                foreach (var attribute in resourceObject.Attributes)
                {
                    var resourceAttribute = apiResource.Attributes.Where(a => a.Name == attribute.Key).FirstOrDefault();
                    if (resourceAttribute == null) continue;

                    var targetProperty = targetType.GetProperty(resourceAttribute.PropertyName);
                    if (targetProperty == null) continue;

                    targetProperty.SetValueFast(result, attribute.Value);
                }
            }

            // extract relationships
            if (resourceObject.Relationships != null)
            {
                foreach (var relationship in resourceObject.Relationships)
                {
                    var relationResource = apiResource.Relationships.Where(r => r.Name == relationship.Key).FirstOrDefault();
                    if (relationResource.Kind == RelationshipKind.BelongsTo)
                    {
                        var relationshipObject = relationship.Value as JsonApiToOneRelationshipObject;
                        if (!string.IsNullOrWhiteSpace(relationshipObject.Data.Id)) // this must not be null
                        {
                            var idProp = targetType.GetProperty(relationResource.IdPropertyName);
                            if (idProp != null)
                            {
                                var idObject = BtbrdCoreIdConverters.ConvertFromString(relationshipObject.Data.Id, idProp.PropertyType);
                                idProp.SetValueFast(result, idObject);
                            }
                        }
                    }
                    else
                    {
                        var idProp = targetType.GetProperty(relationResource.IdPropertyName);
                        // get inner type
                        var innerType = idProp.PropertyType.GenericTypeArguments[0];
                        var relationshipObject = relationship.Value as JsonApiToManyRelationshipObject;
                        // create List instance
                        var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(innerType)) as IList;
                        relationshipObject.Data.ForEach(i => listInstance.Add(BtbrdCoreIdConverters.ConvertFromString(i.Id, innerType)));
                        idProp.SetValueFast(result, listInstance);
                    }
                }
            }
            
            return result;
        }

        #endregion

        #region SetIdAndType

        internal static void SetIdAndType(this JsonApiResourceObject resourceObject, object data, JsonApiResource apiResource)
        {
            resourceObject.Type = apiResource.ResourceType;
            var idData = GetApiResourceId(data, apiResource);
            if(idData != null)
            {
                resourceObject.Id = BtbrdCoreIdConverters.ConvertToString(idData);
            }
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
