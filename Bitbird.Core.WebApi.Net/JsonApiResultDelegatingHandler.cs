using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.Net.Query;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Query;

namespace Bitbird.Core.WebApi.Net
{
    public class JsonApiResultDelegatingHandler : DelegatingHandler
    {
        public readonly JsonApiConfiguration Config;

        public JsonApiResultDelegatingHandler(JsonApiConfiguration config)
        {
            Config = config;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await base.SendAsync(request, cancellationToken);

            if (!(request.Properties.TryGetValue(nameof(JsonApiAttribute.ReturnResource), out var resourceUntyped) && resourceUntyped is JsonApiResource resource))
                return result;

            if (!result.IsSuccessStatusCode)
                return result;

            if (!(result.Content is ObjectContent objectContent))
                return result;

            var meta = new JsonApiMetaData();

            BenchmarkCollection benchmarks = null;
            if (request.Properties.TryGetValue(nameof(BenchmarkCollection), out var benchmarksObj) &&
                benchmarksObj is BenchmarkCollection foundBenchmarks)
            {
                benchmarks = foundBenchmarks;
            }

            try
            {
                Func<ObjectContent> createContent;

                using (benchmarks.CreateBenchmark("ConvertToJsonApiDoc"))
                {
                    var value = objectContent.Value;
                    if (value is IQueryResult queryResult)
                    {
                        meta.PageCount = queryResult.PageCount;
                        meta.RecordCount = queryResult.RecordCount;
                        value = queryResult.Data;
                    }

                    IJsonApiDocument document;
                    if (value is IEnumerable collectionValue)
                    {
                        var apiCollectionDocument = new JsonApiCollectionDocument();
                        apiCollectionDocument.FromApiResource(collectionValue, resource);
                        createContent = () => new ObjectContent<JsonApiCollectionDocument>(apiCollectionDocument, Config.Formatter);
                        document = apiCollectionDocument;
                    }
                    else
                    {
                        var apiDocument = new JsonApiDocument();
                        apiDocument.FromApiResource(value, resource);
                        createContent = () => new ObjectContent<JsonApiDocument>(apiDocument, Config.Formatter);
                        document = apiDocument;
                    }

                    if (request.Properties.TryGetValue(nameof(QueryInfo), out var queryInfoUntyped) && queryInfoUntyped is QueryInfo queryInfo && queryInfo.Includes != null)
                        foreach (var include in queryInfo.Includes)
                            document.IncludeRelation(resource, value, include);

                    document.Meta = meta;
                }

                meta.Benchmarks = benchmarks?.Benchmarks?.Select(b => $"{b.Name}:{b.Duration}");

                result.Content = createContent();

                return result;
            }
            catch (Exception e)
            {
                return e.ToJsonApiErrorResponseMessage();
            }
        }
    }
}