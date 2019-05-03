using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.WebApi.JsonApi;
using Bitbird.Core.WebApi.Models;

namespace Bitbird.Core.WebApi.Resources
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
        }
    }
}