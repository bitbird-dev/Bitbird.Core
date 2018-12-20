using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.JsonDataModel.Attributes;
using Bitbird.Core.Json.Helpers.JsonDataModel.Utils;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.Helpers.JsonDataModel.Extensions
{
    public static class JsonApiResourceObjectExtensions
    {

        #region FromObject

        public static JsonApiResourceObject FromObject(this JsonApiResourceObject resourceObject, IJsonApiDataModel dataModel, bool automaticallyProcessAllRelations = true)
        {
            return JsonApiResourceBuilder.Build(dataModel, automaticallyProcessAllRelations);
        }

        #endregion

        #region ToObject

        /// <summary>
        /// Converts the resource to a object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ToObject<T>(this JsonApiResourceObject resourceObject, bool processRelations = true) where T : IJsonApiDataModel
        {
            return (T)resourceObject.ToObject(typeof(T), processRelations);
        }

        /// <summary>
        /// Converts the resource to a object.
        /// </summary>
        /// <typeparam name="t"></typeparam>
        /// <returns></returns>
        public static IJsonApiDataModel ToObject(this JsonApiResourceObject resourceObject, Type t, bool processRelations = true)
        {

            IJsonApiDataModel result = null;

            // try to create object from attributes
            try
            {
                if (resourceObject.Attributes == null) { result = Activator.CreateInstance(t) as IJsonApiDataModel; }
                else { result = JObject.FromObject(resourceObject.Attributes).ToObject(t) as IJsonApiDataModel; }
                result.SetIdFromString(resourceObject.Id);
            }
            catch { }
            if (!processRelations || result == null || resourceObject.Relationships == null || resourceObject.Relationships.Count < 1) { return result; }

            var idPropertyDict = t.GetProperties().Where(p => p.GetCustomAttribute<JsonApiRelationIdAttribute>() != null).ToDictionary(k => k.GetCustomAttribute<JsonApiRelationIdAttribute>().PropertyName, v => v);
            Dictionary<string, PropertyInfo> refPropertyDict = null;
            Dictionary<string, string> refKeyToName = null;
            {
                var refProperties = t.GetProperties().Where(p => typeof(IJsonApiDataModel).IsAssignableFrom(p.PropertyType) || typeof(IEnumerable<IJsonApiDataModel>).IsAssignableFrom(p.PropertyType));
                refPropertyDict = refProperties.ToDictionary(k => StringUtils.GetRelationShipName(k), v => v);
                refKeyToName = refProperties.ToDictionary(k => StringUtils.GetRelationShipName(k), v => v.Name);
            }

            foreach (var relation in resourceObject.Relationships)
            {
                string propname;
                if (refKeyToName.TryGetValue(relation.Key, out propname))
                {
                    PropertyInfo propertyInfo = null;
                    if (idPropertyDict.TryGetValue(propname, out propertyInfo))
                    {
                        if (propertyInfo.PropertyType is IEnumerable)
                        {
                            var relationConcrete = relation.Value as JsonApiToManyRelationshipObject;
                            if (relationConcrete.Data != null)
                            {
                                propertyInfo.SetValue(result, relationConcrete.Data.Select(r => StringUtils.ConvertId(r.Id, propertyInfo.PropertyType)));
                            }
                        }
                        else
                        {
                            var relationConcrete = relation.Value as JsonApiToOneRelationshipObject;
                            if (relationConcrete.Data != null)
                            {
                                propertyInfo.SetValue(result, StringUtils.ConvertId(relationConcrete.Data.Id, propertyInfo.PropertyType));
                            }
                        }
                    }
                }

                PropertyInfo refInfo;
                if (refPropertyDict.TryGetValue(relation.Key, out refInfo))
                {
                    if (refInfo.PropertyType.IsNonStringEnumerable())
                    {
                        var innerType = refInfo.PropertyType.GenericTypeArguments[0];
                        var constructedListType = typeof(List<>).MakeGenericType(innerType);
                        var collection = Activator.CreateInstance(constructedListType) as IList;
                        var relationConcrete = relation.Value as JsonApiToManyRelationshipObject;
                        foreach (var r in relationConcrete.Data)
                        {
                            var item = Activator.CreateInstance(innerType) as IJsonApiDataModel;
                            item.SetIdFromString(r?.Id);
                            collection.Add(item);
                        }
                        refInfo.SetValue(result, collection);
                    }
                    else
                    {
                        var relationConcrete = relation.Value as JsonApiToOneRelationshipObject;
                        var item = Activator.CreateInstance(refInfo.PropertyType) as IJsonApiDataModel;
                        item.SetIdFromString(relationConcrete.Data.Id);
                        refInfo.SetValue(result, item);
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
