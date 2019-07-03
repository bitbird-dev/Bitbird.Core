using System;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;

namespace Bitbird.Core.Web.AzureFunctions.V2.NetCore
{
    public interface IFunctionResultContent
    {
        object Data { get; }
        Type DataType { get; }
        JsonApiResource Resource { get; }
        QueryInfo QueryInfo { get; }
    }
}