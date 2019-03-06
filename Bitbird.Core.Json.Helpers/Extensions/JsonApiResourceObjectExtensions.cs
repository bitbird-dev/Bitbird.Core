using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.ApiResource.UrlPathBuilder;
using Bitbird.Core.Json.Helpers.Base.Converters;
using Bitbird.Core.Json.JsonApi;
using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    public static class JsonApiResourceObjectExtensions
    {
        internal static void FromApiResource(this JsonApiResourceObject resourceObject, object data, JsonApiResource apiResource, string baseUrl)
        {
            resourceObject.SetIdAndType(data, apiResource);
            if(apiResource?.Attributes != null) {
                foreach (var attr in apiResource.Attributes)
                {
                    resourceObject.AddAttribute(data, attr);
                }
            }
            if (apiResource?.Relationships != null)
            {
                foreach (var realtionship in apiResource.Relationships)
                {
                    if (realtionship.Kind == RelationshipKind.BelongsTo)
                    {
                        resourceObject.AddToOneRelationship(data, apiResource, realtionship, baseUrl);
                    }
                    else
                    {
                        resourceObject.AddToManyRelationship(data, apiResource, realtionship, baseUrl);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                resourceObject.Links = new JsonApiLinksObject
                {
                    Self = new JsonApiLink
                    {
                        Href = $"{baseUrl}{apiResource.UrlPath}/{resourceObject.Id}"
                    }
                };
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

            // TODO: better iterate over attributes and relations defined in the apiresource

            // extract attributes
            if(resourceObject.Attributes != null)
            {
                foreach (var attribute in resourceObject.Attributes)
                {
                    var resourceAttribute = apiResource.Attributes.Where(a => a.Name == attribute.Key).FirstOrDefault();
                    if (resourceAttribute == null) continue;

                    var targetProperty = targetType.GetProperty(resourceAttribute.PropertyName);
                    if (targetProperty == null) continue;

                    // Not handled: ienumerables of datetime,datetimeoffset and nullables

                    var underlying = Nullable.GetUnderlyingType(targetProperty.PropertyType) ?? targetProperty.PropertyType;

                    var value = attribute.Value;
                    if (value != null)
                    {
                        if (underlying == typeof(DateTime))
                            value = DateTime.Parse(value.ToString());
                        else if (underlying == typeof(DateTimeOffset))
                            value = DateTimeOffset.Parse(value.ToString());
                        else if (underlying.IsEnum)
                            value = Enum.ToObject(underlying, Convert.ChangeType(value, Enum.GetUnderlyingType(underlying)));
                        else
                            value = Convert.ChangeType(value, underlying);
                    }

                    targetProperty.SetValueFast(result, value);
                }
            }

            // extract relationships
            if (resourceObject.Relationships != null)
            {
                foreach (var relationship in resourceObject.Relationships)
                {
                    var relationResource = apiResource.Relationships.Where(r => r.Name == relationship.Key).FirstOrDefault();
                    if (relationResource == null) continue;
                    if (relationResource.Kind == RelationshipKind.BelongsTo)
                    {
                        var relationshipObject = relationship.Value as JsonApiToOneRelationshipObject;
                        if (!string.IsNullOrWhiteSpace(relationshipObject.Data?.Id))
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
                        var idProp = targetType.GetProperty(relationResource.IdPropertyName)
                            ?? throw new Exception($"{nameof(JsonApiResourceObjectExtensions)}: Could not find relation property {relationResource.IdPropertyName}");

                        // get type of the id (e.g. long, string, ..)
                        Type innerType;
                        if (idProp.PropertyType.IsArray)
                            innerType = idProp.PropertyType.GetElementType();
                        else if (idProp.PropertyType.IsNonStringEnumerable())
                            innerType = idProp.PropertyType.GenericTypeArguments[0];
                        else
                            throw new Exception($"{nameof(JsonApiResourceObjectExtensions)}: Trying to read the relation, could not find element-type of type {idProp.PropertyType.FullName}.");
                        
                        if (!(relationship.Value is JsonApiToManyRelationshipObject relationshipObject))
                            throw new Exception($"{nameof(JsonApiResourceObjectExtensions)}: Expected a {nameof(JsonApiToManyRelationshipObject)}, found {relationship.Value?.GetType().FullName ?? "null"}");

                        // create List instance
                        //   get the below defined method GetIdCollection for T=innerType
                        //   and executes it.
                        var instance = (typeof(JsonApiResourceObjectExtensions)
                            .GetMethod(nameof(GetIdCollection))
                            ?.MakeGenericMethod(innerType) ?? throw new Exception($"{nameof(JsonApiResourceObjectExtensions)}: Method {nameof(GetIdCollection)} not found."))
                            .Invoke(null, new object[]
                            {
                                /* IEnumerable<JsonApiResourceIdentifierObject> ids : */ relationshipObject.Data,
                                /* bool makeArray : */ idProp.PropertyType.IsArray
                            });

                        idProp.SetValueFast(result, instance);
                    }
                }
            }
            
            return result;
        }
        public static object GetIdCollection<T>(IEnumerable<JsonApiResourceIdentifierObject> ids, bool makeArray)
        {
            var @base = ids.Select(id => BtbrdCoreIdConverters.ConvertFromString<T>(id.Id));

            if (makeArray)
                return @base.ToArray();
            return @base.ToList();
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
            return propertyInfo?.GetValueFast(data);
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

        internal static void AddToOneRelationship(this JsonApiResourceObject resourceObject, object data, JsonApiResource parentResource, ResourceRelationship relationship, string baseUrl)
        {
            string id = null;
            if (string.IsNullOrWhiteSpace(relationship.IdPropertyName))
            {
                var propertyInfo = data.GetType().GetProperty(relationship.PropertyName);
                var value = propertyInfo.GetValueFast(data);
                if (value != null)
                {
                    id = BtbrdCoreIdConverters.ConvertToString(GetApiResourceId(value, relationship.RelatedResource));
                }
            }
            else
            {
                var idPropertyInfo = data.GetType().GetProperty(relationship.IdPropertyName);
                var value = idPropertyInfo?.GetValueFast(data);
                if (value != null)
                {
                    id = BtbrdCoreIdConverters.ConvertToString(value);
                }
            }
            
            if (resourceObject.Relationships == null)
            {
                resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>();
            }
            JsonApiToOneRelationshipObject relation = new JsonApiToOneRelationshipObject();
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                relation.Links = new JsonApiRelationshipLinksObject
                {
                    Self = new JsonApiLink
                    {
                        Href = $"{baseUrl}{parentResource.UrlPath}/{resourceObject.Id}/relationships/{relationship.UrlPath}"
                    }
                };
            }
            if (id != null)
            {
                relation.Data = new JsonApiResourceIdentifierObject
                {
                    Id = id,
                    Type = relationship.RelatedResource.ResourceType
                };
                //relation.Links.Related = new JsonApiLink
                //{
                //    Href = $"<BASE>{relationship.RelatedResource.UrlPath}/{id}"
                //};
            }

            resourceObject.Relationships.Add(relationship.Name, relation);
        }

        #endregion

        #region AddToManyRelationship

        internal static void AddToManyRelationship(this JsonApiResourceObject resourceObject, object data, JsonApiResource parentResource, ResourceRelationship relationship, string baseUrl)
        {
            IEnumerable<string> ids = null;
            if (string.IsNullOrWhiteSpace(relationship.IdPropertyName))
            {
                var propertyInfo = data.GetType().GetProperty(relationship.PropertyName);
                var values = propertyInfo.GetValueFast(data) as IEnumerable<object>;
                if(values != null)
                {
                    ids = values?.Select(x=> BtbrdCoreIdConverters.ConvertToString(GetApiResourceId(x, relationship.RelatedResource)));
                }
            }
            else
            {
                var idPropertyInfo = data.GetType().GetProperty(relationship.IdPropertyName);

                var values = idPropertyInfo?.GetValueFast(data) as IEnumerable;
                if (values != null)
                {
                    var list = new List<string>();
                    foreach (var id in values) { list.Add(BtbrdCoreIdConverters.ConvertToString(id)); }
                    ids = list.AsEnumerable();
                }
            }
            
            if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
            JsonApiToManyRelationshipObject relationshipCollection = new JsonApiToManyRelationshipObject();
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                relationshipCollection.Links = new JsonApiRelationshipLinksObject
                {
                    Self = new JsonApiLink
                    {
                        Href = $"{baseUrl}{parentResource.UrlPath}/{resourceObject.Id}/relationships/{relationship.UrlPath}"
                    }
                };
            }
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
