using Bitbird.Core.Json.Helpers.ApiResource;

namespace Bitbird.Core.WebApi.Net
{
    public interface IJsonApiResourceController
    {
        JsonApiResource GetJsonApiResourceById(string id);
    }
}