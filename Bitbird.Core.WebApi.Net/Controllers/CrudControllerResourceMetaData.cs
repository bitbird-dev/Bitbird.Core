using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.WebApi.JsonApi;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Controllers
{
    public class CrudControllerResourceMetaData
    {
        public readonly Dictionary<Type, Dictionary<string, IControllerRelation>> ControllerRelations = new Dictionary<Type, Dictionary<string, IControllerRelation>>();

        private static IControllerRelation CreateControllerRelation<TModel>([NotNull] ResourceRelationship resourceRelationship)
        {
            switch (resourceRelationship.Kind)
            {
                case RelationshipKind.BelongsTo:
                    return new ControllerToOneRelation<TModel>(resourceRelationship.PropertyName, resourceRelationship.IdPropertyName,
                        resourceRelationship.RelatedResource.ResourceType);
                case RelationshipKind.HasMany:
                    return new ControllerToManyRelation<TModel>(resourceRelationship.PropertyName, resourceRelationship.IdPropertyName,
                        resourceRelationship.RelatedResource.ResourceType);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ResourceRelationship.Kind),
                        $@"{nameof(ResourceRelationship)}.{nameof(ResourceRelationship.Kind)} does not have a valid value.");
            }
        }

        public void ReigsterJsonApiResourceAssemblyByType([NotNull] Type type)
        {
            var entries = Assembly.GetAssembly(type)
                .GetTypes()
                .Where(t => typeof(JsonApiResource).IsAssignableFrom(t))
                .Select(t => new
                {
                    ResourceType = t,
                    ModelType = t.GetCustomAttribute<JsonApiResourceMappingAttribute>()?.Type,
                    Instance = (JsonApiResource)Activator.CreateInstance(t),
                    IsEntity = !(t.GetCustomAttribute<JsonApiResourceMappingAttribute>()?.IsForDataTransferOnly ?? true)
                })
                .Where(t => t.ModelType != null && t.IsEntity)
                .GroupBy(r => r.ModelType)
                .ToDictionary(r => r.Key, r => r.First().Instance.Relationships
                    .ToDictionary(r1 => r1.UrlPath.Trim('/', '\\').ToLowerInvariant(),
                        r1 =>
                        {
                            var methodInfo = typeof(CrudControllerResourceMetaData).GetMethod(nameof(CreateControllerRelation), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"Could not find method {nameof(CrudControllerResourceMetaData)}.{nameof(CreateControllerRelation)}.");
                            return (IControllerRelation)methodInfo.MakeGenericMethod(r.Key).Invoke(null, new object[] {r1});
                        }));

            // merge
            foreach (var entry in entries)
            {
                if (!ControllerRelations.TryGetValue(entry.Key, out var value))
                    ControllerRelations.Add(entry.Key, value = entry.Value);

                foreach (var entry2 in entry.Value)
                {
                    if (!value.TryGetValue(entry2.Key, out var value2))
                        value.Add(entry2.Key, entry2.Value);
                }
            }
        }

        public Func<string, IControllerRelation<TModel>> ForModel<TModel>()
        {
            if (!ControllerRelations.TryGetValue(typeof(TModel), out var forType))
                throw new Exception($"Model {typeof(TModel).Name} does not define relations.");

            return relationName =>
            {
                if (!forType.TryGetValue(relationName.ToLowerInvariant(), out var relation))
                    return null;

                return (IControllerRelation<TModel>) relation;
            };
        }

        public Dictionary<string, IControllerRelation> AllForModel(Type tModel)
        {
            if (!ControllerRelations.TryGetValue(tModel, out var forType))
                throw new Exception($"Model {tModel.Name} does not define relations.");

            return forType;
        }


        public static readonly CrudControllerResourceMetaData Instance = new CrudControllerResourceMetaData();
    }
}