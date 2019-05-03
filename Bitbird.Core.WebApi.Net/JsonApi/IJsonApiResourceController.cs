using Bitbird.Core.Json.Helpers.ApiResource;

namespace Bitbird.Core.WebApi.JsonApi
{
    public interface IJsonApiResourceController
    {
        JsonApiResource GetJsonApiResourceById(string id);
    }
}