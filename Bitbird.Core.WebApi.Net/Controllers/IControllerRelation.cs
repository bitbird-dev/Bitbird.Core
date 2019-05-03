using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.WebApi.Controllers
{
    public interface IControllerRelation
    {

        [NotNull] string Type { get; }
        [NotNull] string IdPropertyName { get; }
        [NotNull] string PropertyName { get; }
        bool IsToMany { get; }
    }

    public interface IControllerRelation<in TModel> : IControllerRelation
    {
        [NotNull]
        object ReadIdModels([NotNull] TModel model);
        void UpdateModel([NotNull] TModel model, [NotNull] object idModels);
        [NotNull]
        object IdModelsFromData([NotNull] JToken data);
    }
}