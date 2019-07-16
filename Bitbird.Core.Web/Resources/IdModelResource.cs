using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Web.JsonApi;
using Bitbird.Core.Web.Models;

namespace Bitbird.Core.Web.Resources
{
    /// <summary>
    /// Describes <see cref="IdModel"/> for various controllers.
    /// Does not provide an url or a type.
    /// </summary>
    [JsonApiResourceMapping(typeof(IdModel), true)]
    public class IdModelResource : JsonApiResource
    {
        /// <inheritdoc/>
        public IdModelResource()
        {
            WithId(nameof(IdModel.Id));

            OfType("id-model", "/id-models");
        }
    }
}