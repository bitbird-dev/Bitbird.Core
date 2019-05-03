using System;
using System.Linq;
using System.Linq.Expressions;
using Bitbird.Core.WebApi.Models;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.WebApi.Controllers
{
    public class ControllerToManyRelation<TModel> : IControllerRelation<TModel>
    {
        [NotNull]
        private readonly Func<TModel, long[]> idsSelector;
        [NotNull]
        private readonly Action<TModel, long[]> idsSetter;
        [NotNull]
        public string Type { get; }
        [NotNull]
        public string IdPropertyName { get; }
        [NotNull]
        public string PropertyName { get; }
        public bool IsToMany => true;

        public ControllerToManyRelation([NotNull] string propertyName, [NotNull] string idsPropertyName, [NotNull] string type)
        {
            var idsProperty = typeof(TModel).GetProperty(idsPropertyName)
                             ?? throw new Exception($"{typeof(TModel).Name} does not have a property {idsPropertyName}.");

            var param = Expression.Parameter(typeof(TModel), "x");
            Expression body = Expression.Property(param, idsProperty);
            idsSelector = Expression.Lambda<Func<TModel, long[]>>(body, param).Compile();

            var paramModel = Expression.Parameter(typeof(TModel), "x");
            var paramIds = Expression.Parameter(typeof(long[]), "ids");
            body = Expression.Assign(Expression.Property(paramModel, idsProperty), paramIds);
            idsSetter = Expression.Lambda<Action<TModel, long[]>>(body, paramModel, paramIds).Compile();

            Type = type;
            PropertyName = propertyName;
            IdPropertyName = idsPropertyName;
        }

        [NotNull]
        public object ReadIdModels([NotNull] TModel model)
        {
            return idsSelector(model)
                .Select(id => new IdModel { Id = id })
                .ToArray();
        }

        [NotNull]
        public object IdModelsFromData([NotNull] JToken data)
        {
            return data.Children().Select(c => c["id"].Value<long>()).ToArray();
        }

        public void UpdateModel([NotNull] TModel model, [NotNull] object idModels)
        {
            idsSetter(model, (long[])idModels);
        }
    }
}