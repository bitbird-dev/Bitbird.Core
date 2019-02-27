using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Linq;
using Bitbird.Core.Json.Extensions;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    internal class IncludePathNode
    {
        public string PropertyPath { get; set; }
        public ResourceRelationship IncludeApiResourceRelationship {get; set;}
        public IncludePathNode Child { get; set; }
    }

    public static class IJsonApiDocumentExtensions
    {
        #region ToObject

        public static object ToObject(this IJsonApiDocument document, JsonApiResource apiResource, Type targetType)
        {
            switch (document)
            {
                case JsonApiDocument doc:
                    return doc.ToObjectInternal(apiResource, targetType);
                case JsonApiCollectionDocument collDoc:
                    return collDoc.ToObjectInternal(apiResource, targetType);
                default:
                    throw new ArgumentException($"Parameter {nameof(document)} does not have a supported type. (Type={document?.GetType()})");
            }
        }

        public static object ToObject(this IJsonApiDocument document, JsonApiResource apiResource, Type targetType, out Func<int, string, bool> foundAttributes)
        {
            switch (document)
            {
                case JsonApiDocument doc:
                    return doc.ToObjectInternal(apiResource, targetType, out foundAttributes);
                case JsonApiCollectionDocument collDoc:
                    return collDoc.ToObjectInternal(apiResource, targetType, out foundAttributes);
                default:
                    throw new ArgumentException($"Parameter {nameof(document)} does not have a supported type. (Type={document?.GetType()})");
            }
        }

        #endregion

        #region IncludeRelation

        public static void IncludeRelation<TResource>(this IJsonApiDocument document, object data, string path, string baseUrl = null) where TResource : JsonApiResource
        {
            document.IncludeRelation(Activator.CreateInstance<TResource>(), data, path, baseUrl);
        }

        public static void IncludeRelation(this IJsonApiDocument document, JsonApiResource dataApiResource, object data, string path, string baseUrl = null)
        {
            // TODO: include relations for collections

            // parse paths
            var subPaths = path.Split(',');

            if (data is IEnumerable<object> collection)
            {
                foreach (var item in collection)
                {
                    foreach (var includePath in subPaths)
                    {
                        // generate tree
                        var includePathTree = GenerateIncludeTree(dataApiResource, includePath);
                        // process tree
                        ProcessIncludeTree(document, includePathTree, item, baseUrl);
                    }
                }
            }
            else
            {
                foreach (var includePath in subPaths)
                {
                    // generate tree
                    var includePathTree = GenerateIncludeTree(dataApiResource, includePath);
                    // process tree
                    ProcessIncludeTree(document, includePathTree, data, baseUrl);
                }
            }
        }

        private static void ProcessIncludeTree(IJsonApiDocument document, IncludePathNode includePathTree, object data, string baseUrl)
        {
            var relationship = includePathTree.IncludeApiResourceRelationship;
            var dataType = data.GetType();
            var propertyInfo = dataType.GetProperty(relationship.PropertyName);
            if (propertyInfo == null) { throw new Exception($"Property {relationship.PropertyName} does not exist for type {dataType.Name}."); }
            var value = propertyInfo.GetValueFast(data);
            if (value == null) { return; }
            if (relationship.Kind == RelationshipKind.BelongsTo)
            {
                var jsonResourceObject = new JsonApiResourceObject();
                jsonResourceObject.FromApiResource(value, relationship.RelatedResource, baseUrl);
                if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                document.Included.AddResource(jsonResourceObject);
                if (includePathTree.Child != null)
                {
                    // jump down the rabbit hole
                    ProcessIncludeTree(document, includePathTree.Child, value, baseUrl);
                }

            }
            else
            {
                if (value is IEnumerable<object> collection && collection.Any())
                {
                    if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                    foreach (var item in collection)
                    {
                        var jsonResourceObject = new JsonApiResourceObject();
                        jsonResourceObject.FromApiResource(item, relationship.RelatedResource, baseUrl);
                        document.Included.AddResource(jsonResourceObject);
                        if (includePathTree.Child != null)
                        {
                            // jump down the rabbit hole
                            ProcessIncludeTree(document, includePathTree.Child, item, baseUrl);
                        }
                    }
                }

            }

        }

        internal static IncludePathNode GenerateIncludeTree(JsonApiResource apiResource, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return null; }
            var subPaths = path.Split(new[] { '.' }, 2);

            var relationshipName = subPaths[0].ToJsonRelationshipName();
            var relationshipResource = apiResource.Relationships.FirstOrDefault(x => x.Name == relationshipName);
            if (relationshipResource == null) { throw new Exception($"Cannot include resource {path}: Path does not exist."); }

            var resultNode = new IncludePathNode
            {
                PropertyPath = relationshipName,
                IncludeApiResourceRelationship = relationshipResource,
                Child = subPaths.Length > 1
                    ? GenerateIncludeTree(relationshipResource.RelatedResource, subPaths[1])
                    : null
            };
            return resultNode;
        }

        #endregion

    }

    public static class ResourceCache
    {
        private static readonly Dictionary<Type, JsonApiResource> Instances = new Dictionary<Type, JsonApiResource>();
        private static readonly Dictionary<Type, Dictionary<string, string>> RelationIdPropertiesToJsonProperties = new Dictionary<Type, Dictionary<string, string>>();

        public static JsonApiResource GetInstance(Type tResource)
        {
            lock (Instances)
            {
                if (Instances.TryGetValue(tResource, out var found))
                    return found;

                try
                {
                    var created = (JsonApiResource)Activator.CreateInstance(tResource);
                    Instances[tResource] = created;
                    return created;
                }
                catch (Exception exception)
                {
                    throw new Exception($"{nameof(ResourceCache)}.{nameof(GetInstance)}: Could not create instance of {tResource.FullName} with no parameters.", exception);
                }
            }
        }
        public static TResource GetInstance<TResource>()
            where TResource : JsonApiResource
        {
            lock (Instances)
            {
                if (Instances.TryGetValue(typeof(TResource), out var found))
                    return (TResource)found;

                try
                {
                    var created = Activator.CreateInstance<TResource>();
                    Instances[typeof(TResource)] = created;
                    return created;
                }
                catch (Exception exception)
                {
                    throw new Exception($"{nameof(ResourceCache)}.{nameof(GetInstance)}: Could not create instance of {typeof(TResource).FullName} with no parameters.", exception);
                }
            }
        }
        public static string GetRelationJsonPropertyNameByIdPropertyName<TResource>(string idPropertyName)
            where TResource : JsonApiResource
        {
            lock (RelationIdPropertiesToJsonProperties)
            {
                if (RelationIdPropertiesToJsonProperties.TryGetValue(typeof(TResource), out var relations))
                    return relations.TryGetValue(idPropertyName, out var found) ? found : null;

                var resource = GetInstance<TResource>();
                var created = resource.Relationships.ToDictionary(r => r.IdPropertyName, r => r.UrlPath.Trim('/', '\\').ToLowerInvariant());
                RelationIdPropertiesToJsonProperties[typeof(TResource)] = created;

                return created[idPropertyName];
            }
        }
        public static string GetRelationJsonPropertyNameByIdPropertyName(Type tResource, string idPropertyName)
        {
            lock (RelationIdPropertiesToJsonProperties)
            {
                if (RelationIdPropertiesToJsonProperties.TryGetValue(tResource, out var relations))
                    return relations.TryGetValue(idPropertyName, out var found) ? found : null;

                var resource = GetInstance(tResource);
                var created = resource.Relationships.ToDictionary(r => r.IdPropertyName, r => r.UrlPath.Trim('/', '\\').ToLowerInvariant());
                RelationIdPropertiesToJsonProperties[tResource] = created;

                return created[idPropertyName];
            }
        }

        public static string ToRelationshipName<TResource>(this string idPropertyName) 
            where TResource : JsonApiResource
        {
            return GetRelationJsonPropertyNameByIdPropertyName<TResource>(idPropertyName)?.ToJsonRelationshipName();
        }
        public static string ToRelationshipName(this string idPropertyName, Type tResource)
        {
            return GetRelationJsonPropertyNameByIdPropertyName(tResource, idPropertyName)?.ToJsonRelationshipName();
        }
    }

    public static class JsonApiCollectionDocumentExtensions
    {
        #region CreateDocumentFromApiResource

        public static JsonApiCollectionDocument CreateDocumentFromApiResource<T>(IEnumerable data, string baseUrl = null) where T : JsonApiResource
        {
            var apiResource = Activator.CreateInstance<T>();
            var document = new JsonApiCollectionDocument();
            document.FromApiResource(data, apiResource, baseUrl);
            return document;
        }

        #endregion

        #region FromApiResource

        public static void FromApiResource(this JsonApiCollectionDocument document, IEnumerable data, JsonApiResource apiResource, string baseUrl = null)
        {
            if (data == null) { return; }
            var resourceObjects = new List<JsonApiResourceObject>();
            foreach (var item in data)
            {
                var rObject = new JsonApiResourceObject();
                rObject.FromApiResource(item, apiResource, baseUrl);
                resourceObjects.Add(rObject);
            }
            document.Data = resourceObjects;
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                document.Links = new JsonApiLinksObject
                {
                    Self = new JsonApiLink
                    {
                        Href = $"{baseUrl}{apiResource.UrlPath}"
                    }
                };
            }
        }

        #endregion
        
        #region ToObjectCollection

        public static IEnumerable<TResult> ToObjectCollection<TResult, TResource>(this JsonApiCollectionDocument document) where TResource : JsonApiResource
        {
            var primaryResourceObjects = document.Data;
            if (primaryResourceObjects == null) throw new Exception("Json document contains no data.");
            return primaryResourceObjects.Select(r => r.ToObject<TResult, TResource>()).ToList();
        }



        public static IEnumerable<TResult> ToObjectCollection<TResult, TResource>(this JsonApiCollectionDocument document, out Func<int, string, bool> foundAttributes) where TResource : JsonApiResource
        {
            foundAttributes = (idx, attrName) => (document.Data.ElementAt(idx)?.Attributes?.ContainsKey(attrName.ToJsonAttributeName()) ?? false) || (document.Data.ElementAt(idx)?.Relationships?.ContainsKey(attrName.ToRelationshipName<TResource>()) ?? false);

            return document.ToObjectCollection<TResult, TResource>();
        }

        #endregion

        #region ToObjectInternal

        internal static IEnumerable<T> Cast<T>(IEnumerable<JsonApiResourceObject> data, JsonApiResource apiResource)
        {
            return data.Select(r => (T)r.ToObject(apiResource, typeof(T))).ToList();
        }

        /// <summary>
        /// Extracts apiResource to an <see cref="IEnumerable{T}"/> of type targetType.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="apiResource"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        internal static object ToObjectInternal(this JsonApiCollectionDocument document, JsonApiResource apiResource, Type targetType)
        {
            if (targetType.IsNonStringEnumerable())
            {
                throw new Exception("Do not use a collection as target type!");
            }
            var primaryResourceObjects = document.Data;
            if (primaryResourceObjects == null) throw new Exception("Json document contains no data.");

            var method = typeof(JsonApiCollectionDocumentExtensions).GetMethod(nameof(Cast), BindingFlags.Static | BindingFlags.NonPublic)?.MakeGenericMethod(targetType);
            return method.Invoke(null, new object[] { primaryResourceObjects, apiResource });
        }

        internal static object ToObjectInternal(this JsonApiCollectionDocument document, JsonApiResource apiResource, Type targetType, out Func<int, string, bool> foundAttributes)
        {
            foundAttributes = (idx, attrName) => (document.Data.ElementAt(idx)?.Attributes?.ContainsKey(attrName.ToJsonAttributeName()) ?? false) || (document.Data.ElementAt(idx)?.Relationships?.ContainsKey(attrName.ToRelationshipName(apiResource.GetType())) ?? false);
            return document.ToObjectInternal(apiResource, targetType);
        }

        #endregion

        #region GetIncludedResource

        public static TResult GetIncludedResource<TResult, TResultApiResource>(this JsonApiCollectionDocument document, object id) where TResultApiResource : JsonApiResource where TResult : class
        {
            return (TResult)document.GetIncludedResource(id, typeof(TResult), Activator.CreateInstance<TResultApiResource>());
        }

        public static TResult GetIncludedResource<TResult>(this JsonApiCollectionDocument document, object id, JsonApiResource apiResource) where TResult : class
        {
            return (TResult)document.GetIncludedResource(id, typeof(TResult), apiResource);
        }

        public static object GetIncludedResource(this JsonApiCollectionDocument document, object id, Type type, JsonApiResource apiResource)
        {
            return document.Included?.GetResource(id, apiResource)?.ToObject(apiResource, type);
        }

        #endregion

    }

    public static class JsonApiDocumentExtensions
    {

        #region CreateDocumentFromApiResource

        public static JsonApiDocument CreateDocumentFromApiResource<T>(object data, string baseUrl = null) where T : JsonApiResource
        {
            var apiResource = Activator.CreateInstance<T>();
            var document = new JsonApiDocument();
            document.FromApiResource(data, apiResource, baseUrl);
            return document;
        }

        #endregion

        #region FromApiResource

        public static void FromApiResource(this JsonApiDocument document, object data, JsonApiResource apiResource, string baseUrl = null)
        {
            switch (data)
            {
                case null:
                    return;
                case IEnumerable<object> _:
                    throw new Exception("data cannot be a collection");
            }

            var rObject = new JsonApiResourceObject();
            rObject.FromApiResource(data, apiResource, baseUrl);
            document.Data = rObject;
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                document.Links = new JsonApiLinksObject
                {
                    Self = new JsonApiLink
                    {
                        Href = $"{baseUrl}{apiResource.UrlPath}/{rObject.Id}"
                    }
                };
            }
        }

        #endregion

        #region ToObject

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <typeparam name="TResult">The Type of the extracted Data.</typeparam>
        /// <typeparam name="TResource">The Type of JsonApiResource used to extract the Data.</typeparam>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// ModelType m = jsonDocument.ToObject&lt;ModelType, ModelTypeApiResource>();
        /// </code>
        /// </example>
        public static TResult ToObject<TResult, TResource>(this JsonApiDocument document) where TResource : JsonApiResource
        {
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            // extract primary data
            return primaryResourceObject.ToObject<TResult, TResource>();
        }

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <typeparam name="TResult">The Type of the extracted Data.</typeparam>
        /// <typeparam name="TResource">The Type of JsonApiResource used to extract the Data.</typeparam>
        /// <param name="foundAttributes">A function to determine which attributes were found in the JsonDocument's primary data</param>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// Func&lt;string, bool&gt; foundAttributes;
        /// ModelType m = jsonDocument.ToObject&lt;ModelType, ModelTypeApiResource&gt;(out foundAttributes);
        /// </code>
        /// </example>
        public static TResult ToObject<TResult, TResource>(this JsonApiDocument document, out Func<string, bool> foundAttributes) where TResource : JsonApiResource
        {
            foundAttributes = (attrName) => (document.Data.Attributes?.ContainsKey(attrName.ToJsonAttributeName()) ?? false) || (document.Data.Relationships?.ContainsKey(attrName.ToRelationshipName<TResource>()) ?? false);

            return document.ToObject<TResult, TResource>();
        }

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <param name="apiResource">instance of JsonApiResource used to extract the Data.</param>
        /// <param name="targetType">The Type of the extracted Data.</param>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// var collection = jsonDocument.ToObject(Activator.CreateInstance&lt;ModelTypeApiResource&gt;(), typeof(IEnumerable&lt;ModelType&gt;));
        /// var singleItem = jsonDocument.ToObject(Activator.CreateInstance&lt;ModelTypeApiResource&gt;(), typeof(ModelType));
        /// </code>
        /// </example>
        public static object ToObjectInternal(this JsonApiDocument document, JsonApiResource apiResource, Type targetType)
        {
            if (targetType.IsNonStringEnumerable())
            {
                var innerType = targetType.GenericTypeArguments[0];
                return document.ToObjectCollection(apiResource, innerType);
            }
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            // extract primary data
            return primaryResourceObject.ToObject(apiResource, targetType);
        }

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <param name="apiResource">instance of JsonApiResource used to extract the Data.</param>
        /// <param name="targetType">The Type of the extracted Data.</param>
        /// <param name="foundAttributes">A function to determine which attributes were found in the JsonDocument's primary data</param>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// Func&gt;string, bool&lt; foundAttributes;
        /// var collection = jsonDocument.ToObject(Activator.CreateInstance&lt;ModelTypeApiResource&gt;(), typeof(IEnumerable&lt;ModelType&gt;, out foundAttributes));
        /// var singleItem = jsonDocument.ToObject(Activator.CreateInstance&lt;ModelTypeApiResource&gt;(), typeof(ModelType), out foundAttributes);
        /// </code>
        /// </example>
        public static object ToObjectInternal(this JsonApiDocument document, JsonApiResource apiResource, Type targetType, out Func<int, string, bool> foundAttributes)
        {
            var attrs = document.Data.Attributes;
            var relations = document.Data.Relationships;
            foundAttributes = (idx, attrName) => (attrs?.ContainsKey(attrName.ToJsonAttributeName()) ?? false) ||(relations?.ContainsKey(attrName.ToRelationshipName(apiResource.GetType())) ?? false);
            return document.ToObject(apiResource, targetType);
        }

        #endregion

        #region ToObjectCollection

        public static IEnumerable<TResult> ToObjectCollection<TResult, TResource>(this JsonApiDocument document) where TResource : JsonApiResource
        {
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");
            return new List<TResult> { primaryResourceObject.ToObject<TResult, TResource>() };
        }
        
        public static IEnumerable<TResult> ToObjectCollection<TResult, TResource>(this JsonApiDocument document, out Func<string, bool> foundAttributes) where TResource : JsonApiResource
        {
            foundAttributes = (attrName) => (document.Data.Attributes?.ContainsKey(attrName.ToJsonAttributeName()) ?? false) || (document.Data.Relationships?.ContainsKey(attrName.ToRelationshipName<TResource>()) ?? false);
            return document.ToObjectCollection<TResult, TResource>();
        }

        internal static IEnumerable<T> Cast<T>(JsonApiResourceObject data, JsonApiResource apiResource)
        {
            return new List<T> { (T)data.ToObject(apiResource, typeof(T)) };
        }

        /// <summary>
        /// Extracts apiResource to an <see cref="IEnumerable{T}"/> of type targetType.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="apiResource"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ToObjectCollection(this JsonApiDocument document, JsonApiResource apiResource, Type targetType)
        {
            if (targetType.IsNonStringEnumerable())
            {
                throw new Exception("Do not use a collection as target type!");
            }
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            var method = typeof(JsonApiDocumentExtensions).GetMethod(nameof(Cast), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(targetType);
            return method.Invoke(null, new object[] { primaryResourceObject, apiResource });
        }
        
        public static object ToObjectCollection(this JsonApiDocument document, JsonApiResource apiResource, Type targetType, out Func<string, bool> foundAttributes)
        {
            foundAttributes = (attrName) => (document.Data.Attributes?.ContainsKey(attrName.ToJsonAttributeName()) ?? false) || (document.Data.Relationships?.ContainsKey(attrName.ToRelationshipName(apiResource.GetType())) ?? false);
            return document.ToObjectCollection(apiResource, targetType);
        }

        #endregion

        #region GetIncludedResource

        public static TResult GetIncludedResource<TResult, TResultApiResource>(this JsonApiDocument document, object id) where TResultApiResource : JsonApiResource where TResult : class
        {
            return (TResult)document.GetIncludedResource(id, typeof(TResult), Activator.CreateInstance<TResultApiResource>());
        }

        public static TResult GetIncludedResource<TResult>(this JsonApiDocument document, object id, JsonApiResource apiResource) where TResult : class
        {
            return (TResult)document.GetIncludedResource(id, typeof(TResult), apiResource);
        }

        public static object GetIncludedResource(this JsonApiDocument document, object id, Type type,JsonApiResource apiResource)
        {
            return document.Included?.GetResource(id, apiResource)?.ToObject(apiResource, type);
        }

        #endregion
    }
}
