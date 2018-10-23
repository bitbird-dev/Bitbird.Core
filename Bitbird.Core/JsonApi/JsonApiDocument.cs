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
    /// A document MUST contain at least one of the following top-level members:
    ///
    ///     data: the document’s “primary data”
    ///     errors: an array of error objects
    ///     meta: a meta object that contains non-standard meta-information.
    ///     
    /// The members data and errors MUST NOT coexist in the same document.
    /// 
    /// A document MAY contain any of these top-level members:
    /// 
    ///     jsonapi: an object describing the server’s implementation
    ///     links: a links object related to the primary data.
    ///     included: an array of resource objects that are related to the primary data and/or each other (“included resources”).
    /// 
    /// If a document does not contain a top-level data key, the included member MUST NOT be present either.
    /// 
    /// The top-level links object MAY contain the following members:
    ///     
    ///     self: the link that generated the current response document.
    ///     related: a related resource link when the primary data represents a resource relationship.
    ///     pagination links for the primary data.
    ///     
    /// Primary data MUST be either:
    /// 
    ///     a single resource object, a single resource identifier object, or null, for requests that target single resources
    ///     an array of resource objects, an array of resource identifier objects, or an empty array([]), for requests that target resource collections
    /// 
    /// </summary>
    /// 
    public class JsonApiDocument<T> where T : JsonApiBaseModel
    {
        #region Properties

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<JsonApiResourceObject> Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Errors { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object JsonApi { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Links { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<JsonApiResourceObject> Included { get; set; }

        #endregion

        #region Constructor

        public JsonApiDocument()
        {

        }

        public JsonApiDocument(IEnumerable<T> data)
        {
            SetupResources(data);
        }

        public JsonApiDocument(T data)
        {
            SetupResources(data);
        }

        public JsonApiDocument(IEnumerable<T> data, IEnumerable<PropertyInfo> includedTypes)
        {
            SetupResources(data);
            SetupIncludes(data, includedTypes);
        }

        public JsonApiDocument(T data, IEnumerable<PropertyInfo> includedTypes)
        {
            SetupResources(data);
            SetupIncludes(data, includedTypes);
        }

        #endregion

        #region Init

        private void SetupResources(T data)
        {
            // Fill data array with resource objects
            Data = new List<JsonApiResourceObject>() { new JsonApiResourceObject(data) };
        }

        private void SetupResources(IEnumerable<T> data)
        {
            // Fill data array with resource objects
            Data = data.Select(dataItem => new JsonApiResourceObject(dataItem));
        }

        private void SetupIncludes(T data, IEnumerable<PropertyInfo> includedProperties)
        {
            SetupIncludes(new List<T> { data }, includedProperties);
        }

        private void SetupIncludes(IEnumerable<T> dataSet, IEnumerable<PropertyInfo> includedProperties)
        {
            if (includedProperties.Any())
            {
                Included = new List<JsonApiResourceObject>();
                foreach(var data in dataSet)
                {
                    var propertiesArray = data.GetType().GetProperties();
                    foreach (var propertyInfo in propertiesArray)
                    {
                        if (includedProperties.Any(x=> x.Name == propertyInfo.Name))
                        {
                            if (propertyInfo.PropertyType.IsSubclassOf(typeof(JsonApiBaseModel)))
                            {
                                var rawdata = propertyInfo.GetValue(data) as JsonApiBaseModel;
                                Included.Add(new JsonApiResourceObject(rawdata, false));
                            }
                            else if (propertyInfo.PropertyType.IsNonStringEnumerable())
                            {
                                var rawdata = propertyInfo.GetValue(data) as IEnumerable<JsonApiBaseModel>;
                                if (rawdata != null)
                                {
                                    foreach(var item in rawdata as IEnumerable<JsonApiBaseModel>)
                                    {
                                        Included.Add(new JsonApiResourceObject(item, false));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Data Access

        public IEnumerable<T> ParseData()
        {
            
            List<T> results = new List<T>();
            if (Data == null || !Data.Any()) { return results; }

            Type resultType = typeof(T);
            foreach(var item in Data)
            {
                // process attributes
                T result = item.Attributes.ToObject<T>();
                result.Id = item.Id;

                if (item.Relationships == null) { results.Add(result); continue; }

                foreach(var propertyInfo in resultType.GetProperties())
                {
                    // primitive data is already processed
                    if (propertyInfo.PropertyType.IsPrimitive) continue;
                    
                    // process to-one relation
                    if (propertyInfo.PropertyType.IsSubclassOf(typeof(JsonApiBaseModel)))
                    {
                        var relationshipBase = item.Relationships.Where(r => r.Key == StringUtils.ToSnakeCase(propertyInfo.Name))?.FirstOrDefault().Value;
                        var relatedResource = ParseToOneRelation(result, relationshipBase as JsonApiToOneRelationship, propertyInfo);
                        if(relatedResource == null) { continue; }
                        ParseToOneInclude(result, propertyInfo, relatedResource);
                    }
                    // process to-many relation
                    else if (propertyInfo.PropertyType.IsNonStringEnumerable())
                    {
                        var relationshipBase = item.Relationships.Where(r => r.Key == StringUtils.ToSnakeCase(propertyInfo.Name))?.FirstOrDefault().Value;
                        var relatedResources = ParseToManyRelation(result, relationshipBase as JsonApiToManyRelationship, propertyInfo);
                        if(relatedResources != null)
                        {
                            ParseToManyInclude(result, propertyInfo, relatedResources);
                        }
                    }
                    else continue;
                }
                results.Add(result);
            }

            return results;
        }

        private void ParseToManyInclude(T targetData, PropertyInfo propertyInfo, IEnumerable<JsonApiResourceIdentifierObject> resources)
        {
            var innerType = propertyInfo.PropertyType.GenericTypeArguments[0];
            if(innerType == null) { return; }

            var targetCollection = propertyInfo.GetValue(targetData) as IEnumerable<JsonApiBaseModel>;
            var constructedListType = typeof(List<>).MakeGenericType(innerType);
            var listInstance = Activator.CreateInstance(constructedListType) as IList;
            
            foreach (var resource in resources)
            {
                var targetResource = targetCollection.Where(x => x.Id == resource.Id)?.FirstOrDefault();
                
                var IncludedResource = Included?.Where(x => (x.Id == resource.Id)&&(x.Type == resource.Type))?.FirstOrDefault();
                if (IncludedResource == null) { listInstance.Add(targetResource); continue; }
                var includedData = IncludedResource.Attributes.ToObject(innerType) as JsonApiBaseModel;
                if (includedData == null) { listInstance.Add(targetResource); continue; }
                includedData.Id = resource.Id;
                listInstance.Add(includedData);
            }
            propertyInfo.SetValue(targetData, listInstance);
        }

        private JsonApiBaseModel ParseToOneInclude(T targetData, PropertyInfo propertyInfo, JsonApiResourceIdentifierObject resource)
        {
            var IncludedResource = Included?.Where(x => (x.Id == resource.Id) && (x.Type == resource.Type))?.FirstOrDefault();
            if (IncludedResource == null) return null;
            var includedData = IncludedResource.Attributes.ToObject(propertyInfo.PropertyType) as JsonApiBaseModel;
            if (includedData == null) return null;
            includedData.Id = resource.Id;
            propertyInfo.SetValue(targetData, includedData);
            return includedData;
        }

        /// <summary>
        /// try to parse relationship data into target data property.
        /// </summary>
        /// <param name="targetData"></param>
        /// <param name="relationship"></param>
        /// <param name="propertyInfo"></param>
        private IEnumerable<JsonApiResourceIdentifierObject> ParseToManyRelation(JsonApiBaseModel targetData, JsonApiToManyRelationship relationship, PropertyInfo propertyInfo)
        {
            if (targetData == null || relationship?.Data == null || propertyInfo == null) return null;
            Type innerType = propertyInfo.PropertyType.GenericTypeArguments[0];
            if (innerType == null) return null;

            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(innerType);

            var listInstance = Activator.CreateInstance(constructedListType);
            List<JsonApiResourceIdentifierObject> result = new List<JsonApiResourceIdentifierObject>();
            foreach(var resourceIdentifier in relationship.Data)
            {
                var propertyData = CreatePropertyResource(innerType, resourceIdentifier);
                if (propertyData == null) continue;
                (listInstance as IList).Add(Convert.ChangeType(propertyData, innerType));
                result.Add(resourceIdentifier);
            }
            try
            {
                propertyInfo.SetValue(targetData, listInstance);
            }
            catch ( Exception e)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Creates a JsonApiBaseModel of a certain Type and sets its id to the supplied resource id.
        /// returns null if something went wrong.
        /// </summary>
        /// <param name="propertyType"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        private JsonApiBaseModel CreatePropertyResource( Type propertyType, JsonApiResourceIdentifierObject resource)
        {
            if (propertyType == null || resource == null) return null;
            JsonApiBaseModel result = null;
            try
            {
                result = Activator.CreateInstance(propertyType) as JsonApiBaseModel;
                result.Id = resource.Id;
            }
            catch (Exception) { }
            return result;
        }


        /// <summary>
        /// returns parsed item on success, null otherwise
        /// </summary>
        /// <param name="targetData"></param>
        /// <param name="relationship"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        private JsonApiResourceIdentifierObject ParseToOneRelation(T targetData, JsonApiToOneRelationship relationship, PropertyInfo propertyInfo)
        {
            if (targetData == null || relationship?.Data == null || propertyInfo == null) return null;
            JsonApiBaseModel result = CreatePropertyResource(propertyInfo.PropertyType, relationship.Data);
            try
            { 
                propertyInfo.SetValue(targetData, result);
            }
            catch(Exception e) { return null; }
            return relationship.Data;
        }

        #endregion
    }
}
