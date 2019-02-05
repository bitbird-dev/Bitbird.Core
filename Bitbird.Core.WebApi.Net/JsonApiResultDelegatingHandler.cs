﻿using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.Net.Query;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Query;
using Newtonsoft.Json;

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

            if (!(request.Properties.TryGetValue(nameof(JsonApiAttribute.ReturnResourceGetter), out var resourceUntyped) && resourceUntyped is JsonApiResource resource))
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
                IJsonApiDocument document;

                using (benchmarks.CreateBenchmark("ConvertToJsonApiDoc"))
                {
                    string overridePrimaryType = null;

                    var value = objectContent.Value;
                    if (value is IJsonApiOverridePrimaryType jsonApiOverridePrimaryType)
                    {
                        value = jsonApiOverridePrimaryType.Data;
                        overridePrimaryType = jsonApiOverridePrimaryType.Type;
                    }
                    if (value is IQueryResult queryResult)
                    {
                        meta.PageCount = queryResult.PageCount;
                        meta.RecordCount = queryResult.RecordCount;
                        value = queryResult.Data;
                    }

                    if (value is IEnumerable collectionValue)
                    {
                        var apiCollectionDocument = new JsonApiCollectionDocument();
                        apiCollectionDocument.FromApiResource(collectionValue, resource);
                        if (overridePrimaryType != null)
                            foreach (var primaryDataEntry in apiCollectionDocument.Data)
                                primaryDataEntry.Type = overridePrimaryType;
                        document = apiCollectionDocument;
                    }
                    else
                    {
                        var apiDocument = new JsonApiDocument();
                        apiDocument.FromApiResource(value, resource);
                        if (overridePrimaryType != null)
                            apiDocument.Data.Type = overridePrimaryType;
                        document = apiDocument;
                    }

                    if (request.Properties.TryGetValue(nameof(QueryInfo), out var queryInfoUntyped) && queryInfoUntyped is QueryInfo queryInfo && queryInfo.Includes != null)
                        foreach (var include in queryInfo.Includes)
                            document.IncludeRelation(resource, value, include);

                    document.Meta = meta;
                }

                meta.Benchmarks = benchmarks?.Benchmarks?.Select(b => $"{b.Name}:{b.Duration}");
                
                result.Content = new StringContent(JsonConvert.SerializeObject(document, Config.Formatter.SerializerSettings), Encoding.UTF8, "application/vnd.api+json");

                return result;
            }
            catch (Exception e)
            {
                return e.ToJsonApiErrorResponseMessage();
            }
        }
    }
}