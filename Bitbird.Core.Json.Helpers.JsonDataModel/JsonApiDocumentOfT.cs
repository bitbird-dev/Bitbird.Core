using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.JsonDataModel.Extensions;
using Bitbird.Core.Json.Helpers.JsonDataModel.Utils;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.JsonApi.Dictionaries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.Helpers.JsonDataModel
{
    public class JsonApiDocument<T> : JsonApiDocument where T : IJsonApiDataModel
    {

        #region Constructor

        public JsonApiDocument()
        {
        }

        public JsonApiDocument(IEnumerable<T> data)
        {
            SetupResources(data);
        }

        public JsonApiDocument(IEnumerable<T> data, Uri queryString)
        {
            SetupResources(data, queryString);
            SetupLinks(queryString);
        }

        public JsonApiDocument(T data)
        {
            SetupResources(data);
        }

        public JsonApiDocument(T data, Uri queryString)
        {
            SetupResources(data, queryString);
            SetupLinks(queryString);
        }

        public JsonApiDocument(IEnumerable<T> data, IEnumerable<PropertyInfo> includedTypes)
        {
            SetupResources(data);
            SetupIncludes(data, includedTypes);
        }

        public JsonApiDocument(IEnumerable<T> data, IEnumerable<PropertyInfo> includedTypes, Uri queryString)
        {
            SetupResources(data, queryString);
            SetupIncludes(data, includedTypes, queryString);
            SetupLinks(queryString);
        }

        public JsonApiDocument(T data, IEnumerable<PropertyInfo> includedTypes)
        {
            SetupResources(data);
            SetupIncludes(data, includedTypes);
        }

        public JsonApiDocument(T data, IEnumerable<PropertyInfo> includedTypes, Uri queryString)
        {
            SetupResources(data, queryString);
            SetupIncludes(data, includedTypes, queryString);
            SetupLinks(queryString);
        }

        #endregion

        #region Init

        /// <summary>
        /// Instantiates the ResourceObjects in the Data property
        /// </summary>
        /// <param name="data"></param>
        private void SetupResources(T data, Uri queryString = null)
        {
            // Fill data array with resource objects
            Data = new List<JsonApiResourceObject>() { JsonApiResourceBuilder.Build(data, true) };
        }

        /// <summary>
        /// Instantiates the ResourceObjects in the Data property
        /// </summary>
        /// <param name="data"></param>
        private void SetupResources(IEnumerable<T> data, Uri queryString = null)
        {
            // Fill data array with resource objects
            Data = data.Select(dataItem => JsonApiResourceBuilder.Build(dataItem, true));
        }

        /// <summary>
        /// Populates the Included Property with ResourceObjects
        /// </summary>
        /// <param name="data"></param>
        /// <param name="includedProperties"></param>
        private void SetupIncludes(T data, IEnumerable<PropertyInfo> includedProperties, Uri queryString = null)
        {
            SetupIncludes(new List<T> { data }, includedProperties);
        }

        /// <summary>
        /// Populates the Included Property with ResourceObjects
        /// </summary>
        /// <param name="dataSet">The data conatining the properties</param>
        /// <param name="includedProperties">Defines which properties are to be included</param>
        private void SetupIncludes(IEnumerable<T> dataSet, IEnumerable<PropertyInfo> includedProperties, Uri queryString = null)
        {
            // ignore if no data is present
            if (!includedProperties.Any()) return;

            if (Included == null) { Included = new JsonApiResourceObjectDictionary(); }
            // process each item in data
            foreach (var data in dataSet)
            {
                // iterate over all of the items' properties and look for the requested properties
                var propertiesArray = data.GetType().GetProperties();
                foreach (var propertyInfo in propertiesArray)
                {
                    if (includedProperties.Any(x => x.Name == propertyInfo.Name))
                    {
                        if (typeof(IJsonApiDataModel).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            var rawdata = propertyInfo.GetValue(data) as IJsonApiDataModel;
                            Included.AddResource(JsonApiResourceBuilder.Build(rawdata, false));
                        }
                        else if (propertyInfo.PropertyType.IsNonStringEnumerable())
                        {
                            var rawdata = propertyInfo.GetValue(data) as IEnumerable<IJsonApiDataModel>;
                            if (rawdata != null)
                            {
                                foreach (var item in rawdata as IEnumerable<IJsonApiDataModel>)
                                {
                                    Included.AddResource(JsonApiResourceBuilder.Build(item, false));
                                }
                            }
                        }
                    }
                }
            }
            if (Included.ResourceObjectDictionary.Count < 1) { Included = null; }
        }

        public void SetupLinks(Uri queryString)
        {
            //check if baseUri contains actual information
            if (queryString == null || string.IsNullOrWhiteSpace(queryString.Host)) return;

            // setup link to self
            Links = new JsonApiLinksObject { Self = new JsonApiLink(queryString.AbsoluteUri) };
        }

        #endregion

        #region Data Access

        /// <summary>
        /// Extracts all primary data items from the JsonApiDocuments.
        /// If any Relationships are present the model instances with the respective id's will being injected.
        /// If any Includes are present the model instances with the respective attirbutes will being injected.
        /// </summary>
        /// <returns>An empty collection if no data exists</returns>
        public IEnumerable<T> ToObject()
        {
            List<T> results = new List<T>();
            if (Data == null || !Data.Any()) { return results; }

            Type resultType = typeof(T);
            foreach (var item in Data)
            {
                T result = default(T);
                result = item.ToObject<T>(true);

                if (item.Relationships == null) { results.Add(result); continue; }

                foreach (var property in resultType.GetProperties())
                {
                    if (typeof(IJsonApiDataModel).IsAssignableFrom(property.PropertyType))
                    {
                        var propertyValue = property.GetValueFast(result) as IJsonApiDataModel;
                        if (propertyValue == null) { continue; }
                        string typeString = propertyValue.GetJsonApiClassName();
                        var includedResource = GetIncludedResource(new ResourceKey(propertyValue.GetIdAsString(), typeString));
                        if (includedResource == null) { continue; }
                        property.SetValue(result, includedResource.ToObject(property.PropertyType));
                    }
                    else if (property.PropertyType.IsNonStringEnumerable())
                    {
                        var innerType = property.PropertyType.GenericTypeArguments[0];
                        var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(innerType)) as IList;
                        var propertyValue = property.GetValueFast(result) as IEnumerable<IJsonApiDataModel>;
                        if (propertyValue == null) { continue; }
                        foreach (var reference in propertyValue)
                        {
                            string typeString = reference.GetJsonApiClassName();
                            var includedResource = GetIncludedResource(new ResourceKey(reference.GetIdAsString(), typeString));
                            if (includedResource == null) { listInstance.Add(reference); continue; }
                            listInstance.Add(includedResource.ToObject(reference.GetType()));
                        }
                        property.SetValue(result, listInstance);
                    }
                }
                results.Add(result);
            }

            return results;
        }

        private JsonApiResourceObject GetIncludedResource(ResourceKey includedKey)
        {
            JsonApiResourceObject includedResourceObject = null;
            Included?.ResourceObjectDictionary.TryGetValue(includedKey, out includedResourceObject);
            return includedResourceObject;
        }

        private JsonApiResourceObject GetIncludedResource(JsonApiResourceIdentifierObject resource)
        {
            var includedKey = new ResourceKey(resource.Id, resource.Type);
            JsonApiResourceObject includedResourceObject = null;
            Included?.ResourceObjectDictionary.TryGetValue(includedKey, out includedResourceObject);
            return includedResourceObject;
        }

        #endregion
    }
}
