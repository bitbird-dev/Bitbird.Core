using Bitbird.Core.Extensions;
using Bitbird.Core.JsonApi;
using Bitbird.Core.JsonApi.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Utils
{
    public class JsonApiResourceBuilder
    {
        #region AttributeGroup Enum

        private enum AttributeGroup
        {
            Ignored,
            Unknown,
            Primitive,
            PrimitiveCollection,
            PrimitiveId,
            PrimitiveIdCollection,
            Reference,
            ReferenceCollection
        }

        #endregion

        #region RelationShipInfo

        private class RelationShipInfo
        {
            public string RelationshipKey { get; set; }
            public string RelationshipType { get; set; }
            public HashSet<string> RelationshipIds { get; set; } = new HashSet<string>();

        }

        #endregion

        #region GetAttributeGroup

        private AttributeGroup GetAttributeGroup(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null) { return AttributeGroup.Ignored; }
            
            if (propertyInfo.PropertyType.IsPrimitiveOrString())
            {
                if (propertyInfo.GetCustomAttribute<JsonApiRelationIdAttribute>() != null) { return AttributeGroup.PrimitiveId; }
                else { return AttributeGroup.Primitive; }
            }

            if (typeof(JsonApiBaseModel).IsAssignableFrom(propertyInfo.PropertyType)) { return AttributeGroup.Reference; }

            if (propertyInfo.PropertyType.IsNonStringEnumerable())
            {
                var innerType = propertyInfo.PropertyType.GenericTypeArguments[0];
                if (innerType == null) { return AttributeGroup.Unknown; }
                if (typeof(JsonApiBaseModel).IsAssignableFrom(innerType)) { return AttributeGroup.ReferenceCollection; }
                if (innerType.IsPrimitiveOrString())
                {
                    if (propertyInfo.GetCustomAttribute<JsonApiRelationIdAttribute>() != null) { return AttributeGroup.PrimitiveIdCollection; }
                    { return AttributeGroup.PrimitiveCollection; }
                }
            }

            return AttributeGroup.Unknown;
        }

        #endregion

        #region SetupData

        public JsonApiResourceObject Build(JsonApiBaseModel data, bool processRelations)
        {
            if (data == null) { throw new Exception("data is null"); }

            JsonApiResourceObject result = new JsonApiResourceObject();

            result.Id = data.Id;
            result.Type = data.GetJsonApiClassName();

            var classType = data.GetType();

            // sort out properties
            var propertyInfos = classType.GetProperties();
            var propoertyInfoLookup = propertyInfos.ToLookup(p => GetAttributeGroup(p));

            result.Attributes = new Dictionary<string, object>();
            foreach (var item in propoertyInfoLookup[AttributeGroup.Primitive])
            {
                var value = item.GetValueFast(data);
                if (value == null && item.JsonIsIgnoredIfNull()) { continue; }
                result.Attributes.Add(StringUtils.GetAttributeName(item), value);
            }

            foreach (var item in propoertyInfoLookup[AttributeGroup.PrimitiveCollection])
            {
                var value = item.GetValueFast(data);
                if (value == null && item.JsonIsIgnoredIfNull()) { continue; }
                result.Attributes.Add(StringUtils.GetAttributeName(item), value);
            }

            if (result.Attributes.Count < 1) { result.Attributes = null; }


            if(!processRelations ) { return result; }

            Dictionary<string, RelationShipInfo> relationships = new Dictionary<string, RelationShipInfo>();

            foreach (var item in propoertyInfoLookup[AttributeGroup.Reference])
            {
                var value = item.GetValueFast(data) as JsonApiBaseModel;
                var info = new RelationShipInfo { RelationshipKey = StringUtils.GetRelationShipName(item), RelationshipType = value.GetJsonApiClassName() };
                if (value != null) { info.RelationshipIds.Add(value.Id); }
                relationships.Add(item.Name, info);
            }

            foreach (var item in propoertyInfoLookup[AttributeGroup.ReferenceCollection])
            {
                var value = item.GetValueFast(data) as IEnumerable<JsonApiBaseModel>;
                var info = new RelationShipInfo
                {
                    RelationshipKey = StringUtils.GetRelationShipName(item),
                    RelationshipType = JsonApiBaseModel.GetJsonApiClassName(item.PropertyType.GetGenericArguments()[0])
                };
                if (value != null)
                {
                    foreach (var relation in value)
                    {
                        info.RelationshipIds.Add(relation.Id);
                    }
                }
                relationships.Add(item.Name, info);
            }

            foreach (var item in propoertyInfoLookup[AttributeGroup.PrimitiveId])
            {
                var value = item.GetValueFast(data);
                if (value == null) { continue; }
                RelationShipInfo info = null;
                var idAttr = item.GetCustomAttribute<JsonApiRelationIdAttribute>();
                if (!relationships.TryGetValue(idAttr.PropertyName, out info))
                {
                    throw new Exception($"no reference found for id backing field {idAttr.PropertyName}");
                }
                info.RelationshipIds.Add(value.ToString());
            }

            foreach (var item in propoertyInfoLookup[AttributeGroup.PrimitiveIdCollection])
            {
                var value = item.GetValueFast(data) as IEnumerable;
                if (value == null) { continue; }
                RelationShipInfo info = null;
                var idAttr = item.GetCustomAttribute<JsonApiRelationIdAttribute>();
                if (!relationships.TryGetValue(idAttr.PropertyName, out info))
                {
                    throw new Exception($"no reference found for id backing field {idAttr.PropertyName}");
                }
                foreach (var val in value)
                {
                    info.RelationshipIds.Add(val.ToString());
                }
            }

            foreach (var r in relationships)
            {
                if(r.Value.RelationshipIds.Count < 1) { continue; }
                if (r.Value.RelationshipIds.Count == 1)
                {
                    var relO = new JsonApiToOneRelationshipObject();
                    relO.Data = new JsonApiResourceIdentifierObject { Id = r.Value.RelationshipIds.First(), Type = r.Value.RelationshipType };
                    result.Relationships.Add(r.Value.RelationshipKey, relO);
                }
                else
                {
                    var relO = new JsonApiToManyRelationshipObject();
                    relO.Data = r.Value.RelationshipIds.Select(i => new JsonApiResourceIdentifierObject { Id = i, Type = r.Value.RelationshipType }).ToList();
                    result.Relationships.Add(r.Value.RelationshipKey, relO);
                }
            }
            if (result.Relationships.Count < 1) { result.Relationships = null; }

            return result;
        }

        #endregion
    }
}
