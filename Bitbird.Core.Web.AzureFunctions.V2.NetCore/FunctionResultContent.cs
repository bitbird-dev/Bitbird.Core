using System;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;
using JetBrains.Annotations;

namespace Bitbird.Core.Web.AzureFunctions.V2.NetCore
{
    public class FunctionResultContent<T, TResource>
        : IFunctionResultContent
        where TResource : JsonApiResource
    {
        [CanBeNull]
        public T Data { get; }

        [NotNull]
        public TResource Resource { get; }

        [CanBeNull]
        public QueryInfo QueryInfo { get; }

        [NotNull]
        public Type DataType => typeof(T);

        object IFunctionResultContent.Data => Data;
        JsonApiResource IFunctionResultContent.Resource => Resource;

        public FunctionResultContent([CanBeNull] T data, [NotNull] TResource resource, [CanBeNull] QueryInfo queryInfo)
        {
            Data = data;
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            QueryInfo = queryInfo;
        }
    }
}