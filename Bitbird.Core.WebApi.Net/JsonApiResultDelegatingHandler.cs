using System;
using System.Collections;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

            try
            {
                if (objectContent.Value is IEnumerable collectionValue)
                {
                    var document = new JsonApiCollectionDocument();
                    document.FromApiResource(collectionValue, resource);

                    //if (request.Properties.TryGetValue(nameof(QueryInfo), out var queryInfoUntyped) && queryInfoUntyped is QueryInfo queryInfo && queryInfo.Includes != null)
                        //foreach (var include in queryInfo.Includes)
                            // document.IncludeRelation(resource, objectContent.Value, include); // TODO: include relations for collections

                    result.Content = new ObjectContent<JsonApiCollectionDocument>(document, Config.Formatter);
                }
                else
                {
                    var document = new JsonApiDocument();
                    document.FromApiResource(objectContent.Value, resource);

                    if (request.Properties.TryGetValue(nameof(QueryInfo), out var queryInfoUntyped) && queryInfoUntyped is QueryInfo queryInfo && queryInfo.Includes != null)
                        foreach (var include in queryInfo.Includes)
                            document.IncludeRelation(resource, objectContent.Value, include);

                    result.Content = new ObjectContent<JsonApiDocument>(document, Config.Formatter);
                }

                return result;
            }
            catch (Exception e)
            {
                return e.ToJsonApiErrorResponseMessage();
            }
        }
    }
}