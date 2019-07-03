using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Bitbird.Core.Web.AzureFunctions.V2.NetCore
{
    [UsedImplicitly]
    public class FunctionResult<T, TResource> : ObjectResult
        where TResource : JsonApiResource
    {
        public FunctionResult([CanBeNull] T data, [NotNull] TResource resource, [CanBeNull] QueryInfo queryInfo = null)
            : base(new FunctionResultContent<T, TResource>(data, resource, queryInfo))
        {

        }
    }
}