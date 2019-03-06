using System;
using Bitbird.Core.Json.JsonApi;

namespace Bitbird.Core.WebApi.Net
{
    public interface IContentInfo
    {
        JsonApiDocument Document { get; }
        Func<string, bool> FoundAttributes { get; }
    }
}