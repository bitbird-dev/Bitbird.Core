using System;
using System.Linq.Expressions;
using System.Net;
using System.Web.Http;
using Bitbird.Core.Web.Models;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.WebApi.Controllers
{
    public class ControllerToOneRelation<TModel> : IControllerRelation<TModel>
    {
        [NotNull]
        private readonly Func<TModel, long?> idSelector;
        [NotNull]
        private readonly Action<TModel, long?> idsSetter;
        [NotNull]
        public string Type { get; }
        [NotNull]
        public string IdPropertyName { get; }
        [NotNull]
        public string PropertyName { get; }
        public bool IsToMany => false;

        public ControllerToOneRelation([NotNull] string propertyName, [NotNull] string idPropertyName, [NotNull] string type)
        {
            var idProperty = typeof(TModel).GetProperty(idPropertyName) 
                             ?? throw new Exception($"{typeof(TModel).Name} does not have a property {idPropertyName}.");

            var param = Expression.Parameter(typeof(TModel), "x");
            Expression body = Expression.Convert(Expression.Property(param, idProperty), typeof(long?));
            idSelector = Expression.Lambda<Func<TModel, long?>>(body, param).Compile();


            if (idProperty.PropertyType == typeof(long?))
            {
                var paramModel = Expression.Parameter(typeof(TModel), "x");
                var paramIds = Expression.Parameter(typeof(long?), "id");
                body = Expression.Assign(Expression.Property(paramModel, idProperty), paramIds);
                idsSetter = Expression.Lambda<Action<TModel, long?>>(body, paramModel, paramIds).Compile();
            }
            else
            {
                var paramModel = Expression.Parameter(typeof(TModel), "x");
                var paramIds = Expression.Parameter(typeof(long), "id");
                body = Expression.Assign(Expression.Property(paramModel, idProperty), paramIds);
                var tempSetter = Expression.Lambda<Action<TModel, long>>(body, paramModel, paramIds).Compile();

                idsSetter = (model, ids) =>
                {
                    tempSetter(model, ids ?? throw new HttpResponseException(HttpStatusCode.BadRequest)); // TODO: better error
                };
            }

            Type = type;
            PropertyName = propertyName;
            IdPropertyName = idPropertyName;
        }

        [CanBeNull]
        public object ReadIdModels([NotNull] TModel model)
        {
            var id = idSelector(model);
            return id == null ? null : new IdModel { Id = id };
        }

        [NotNull]
        public object IdModelsFromData([NotNull] JToken data)
        {
            return data["id"].Value<long>();
        }
        public void UpdateModel([NotNull] TModel model, [NotNull] object idModels)
        {
            idsSetter(model, (long?)idModels); // TODO: Handle null
        }
    }
}