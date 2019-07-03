using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Query;
using Bitbird.Core.Web.JsonApi;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.Web.AzureFunctions.V2.NetCore
{
    public static class HttpRequestParserExtensions
    {
        private static readonly Regex FilterKeyRegex = new Regex("filter[.](?<Property>[a-zA-Z0-9_.]+)", RegexOptions.Compiled);
        private static readonly Regex FilterValueRangeRegex = new Regex("RANGE[(](?<LowerBound>.*)[;](?<UpperBound>.*)[)]", RegexOptions.Compiled);
        private static readonly Regex FilterValueInRegex = new Regex("IN[(](?<Values>.*)[)]", RegexOptions.Compiled);
        private static readonly Regex FilterValueFreeTextRegex = new Regex("FREETEXT[(](?<Pattern>.*)[)]", RegexOptions.Compiled);
        private static readonly Regex FilterValueGtLt = new Regex("(?:(?<gte>GTE)|(?<gt>GT)|(?<lte>LTE)|(?<lt>LT))[(](?<Bound>.*)[)]", RegexOptions.Compiled);

        [NotNull, UsedImplicitly]
        public static QueryInfo ParseQueryInfo([NotNull] this HttpRequest httpRequest)
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));

            int? pageNumber = null;
            int? pageSize = null;
            QuerySortProperty[] sort = null;
            string[] includes = null;
            var filter = new List<QueryFilter>();

            foreach (var entry in httpRequest.Query.SelectMany(
                kvp => kvp.Value.Select(value => new
                {
                    /*Key = */kvp.Key,
                    Value = value
                })))
            {
                if (entry.Key.Equals("page.number"))
                {
                    pageNumber = Convert.ToInt32(entry.Value);
                    continue;
                }
                if (entry.Key.Equals("page.size"))
                {
                    pageSize = Convert.ToInt32(entry.Value);
                    continue;
                }
                if (entry.Key.Equals("sort"))
                {
                    sort = entry.Value
                        .Split(',')
                        .Select(f => f.StartsWith("-") ? new QuerySortProperty(f.Substring(1), false) : new QuerySortProperty(f))
                        .ToArray();
                    continue;
                }
                if (entry.Key.Equals("include"))
                {
                    includes = entry.Value
                        .Split(',')
                        .ToArray();
                    continue;
                }

                var match = FilterKeyRegex.Match(entry.Key);
                if (match.Success)
                {
                    var property = match.Groups["Property"].Value;

                    match = FilterValueRangeRegex.Match(entry.Value);
                    if (match.Success)
                    {
                        filter.Add(QueryFilter.Range(property, match.Groups["LowerBound"].Value, match.Groups["UpperBound"].Value));
                        continue;
                    }

                    match = FilterValueGtLt.Match(entry.Value);
                    if (match.Success)
                    {
                        if (match.Groups["gt"].Success)
                            filter.Add(QueryFilter.GreaterThan(property, match.Groups["Bound"].Value));
                        else if (match.Groups["gte"].Success)
                            filter.Add(QueryFilter.GreaterThanEqual(property, match.Groups["Bound"].Value));
                        else if (match.Groups["lt"].Success)
                            filter.Add(QueryFilter.LessThan(property, match.Groups["Bound"].Value));
                        else if (match.Groups["lte"].Success)
                            filter.Add(QueryFilter.LessThanEqual(property, match.Groups["Bound"].Value));
                        continue;
                    }

                    match = FilterValueInRegex.Match(entry.Value);
                    if (match.Success)
                    {
                        filter.Add(QueryFilter.In(property, match.Groups["Values"].Value.Split(';')));
                        continue;
                    }

                    match = FilterValueFreeTextRegex.Match(entry.Value);
                    if (match.Success)
                    {
                        filter.Add(QueryFilter.FreeText(property, match.Groups["Pattern"].Value));
                        continue;
                    }

                    filter.Add(QueryFilter.Exact(property, entry.Value));
                }
            }

            return new QueryInfo(sort,
                filter.Count == 0 ?
                    null :
                    filter.ToArray(),
                pageSize.HasValue ?
                    new QueryPaging(pageSize.Value, pageNumber ?? 0) :
                    null,
                includes);
        }



        [NotNull, ItemNotNull, UsedImplicitly]
        public static Task<ContentInfo<TModel>> ParseModelFromBodyAsync<TModel, TResource>(
            [NotNull] this HttpRequest httpRequest,
            [NotNull] TResource resource)
            where TResource : JsonApiResource
        {
            if (string.Equals(httpRequest.ContentType, "application/json", StringComparison.InvariantCultureIgnoreCase))
                return ParseModelFromJsonBodyAsync<TModel, TResource>(httpRequest, resource);

            if (string.Equals(httpRequest.ContentType, "application/vnd.api+json", StringComparison.InvariantCultureIgnoreCase))
                return ParseModelFromJsonApiBodyAsync<TModel, TResource>(httpRequest, resource);
            
            throw new Exception($"Content type '{httpRequest.ContentType}' not supported.");
        }

        [NotNull, ItemNotNull, UsedImplicitly]
        public static Task<ContentInfo<TModel>[]> ParseModelCollectionFromBodyAsync<TModel, TResource>(
            [NotNull] this HttpRequest httpRequest,
            [NotNull] TResource resource)
            where TResource : JsonApiResource
        {
            if (string.Equals(httpRequest.ContentType, "application/json", StringComparison.InvariantCultureIgnoreCase))
                return ParseModelCollectionFromJsonBodyAsync<TModel, TResource>(httpRequest, resource);

            if (string.Equals(httpRequest.ContentType, "application/vnd.api+json", StringComparison.InvariantCultureIgnoreCase))
                return ParseModelCollectionFromJsonApiBodyAsync<TModel, TResource>(httpRequest, resource);
            
            throw new Exception($"Content type '{httpRequest.ContentType}' not supported.");
        }







        [NotNull, ItemNotNull, UsedImplicitly]
        public static async Task<ContentInfo<TModel>> ParseModelFromJsonApiBodyAsync<TModel, TResource>(
            [NotNull] this HttpRequest httpRequest, 
            [NotNull] TResource resource)
            where TResource : JsonApiResource
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            JToken jToken;
            using (var reader = new StreamReader(httpRequest.Body))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    jToken = await JToken.LoadAsync(jsonReader);
                }
            }

            var document = jToken.ToObject<JsonApiDocument>();
            var model = document.ToObject<TModel, TResource>(resource, out var foundAttributes);
            return new ContentInfo<TModel>(model, foundAttributes);
        }

        [NotNull, ItemNotNull, UsedImplicitly]
        public static async Task<ContentInfo<TModel>[]> ParseModelCollectionFromJsonApiBodyAsync<TModel, TResource>(
            [NotNull] this HttpRequest httpRequest, 
            [NotNull] TResource resource)
            where TResource : JsonApiResource
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            JToken jToken;
            using (var reader = new StreamReader(httpRequest.Body))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    jToken = await JToken.LoadAsync(jsonReader);
                }
            }

            var document = jToken.ToObject<JsonApiCollectionDocument>();
            var model = document.ToObjectCollection<TModel, TResource>(resource, out var foundAttributes);
            return model
                .Select((item, idx) => new ContentInfo<TModel>(item, p => foundAttributes(idx, p)))
                .ToArray();
        }


        [NotNull, ItemNotNull, UsedImplicitly]
        public static async Task<ContentInfo<TModel>> ParseModelFromJsonBodyAsync<TModel, TResource>(
            [NotNull] this HttpRequest httpRequest,
            [NotNull] TResource resource)
            where TResource : JsonApiResource
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            JToken jToken;
            using (var reader = new StreamReader(httpRequest.Body))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    jToken = await JToken.LoadAsync(jsonReader);
                }
            }

            var model = jToken.ToObject<TModel>();
            return new ContentInfo<TModel>(model, p => true);
        }

        [NotNull, ItemNotNull, UsedImplicitly]
        public static async Task<ContentInfo<TModel>[]> ParseModelCollectionFromJsonBodyAsync<TModel, TResource>(
            [NotNull] this HttpRequest httpRequest,
            [NotNull] TResource resource)
            where TResource : JsonApiResource
        {
            if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            JToken jToken;
            using (var reader = new StreamReader(httpRequest.Body))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jsonReader.DateParseHandling = DateParseHandling.None;
                    jToken = await JToken.LoadAsync(jsonReader);
                }
            }

            var model = jToken.ToObject<TModel[]>();
            return model.Select(item => new ContentInfo<TModel>(item, p => true)).ToArray();
        }
    }
}