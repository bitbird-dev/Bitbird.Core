using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;
using Microsoft.Azure;

namespace Bitbird.Core.WebApi.Net
{
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonApiAttribute : ActionFilterAttribute
    {
        private static readonly Regex FilterKeyRegex = new Regex("filter[.](?<Property>[a-zA-Z0-9_.]+)", RegexOptions.Compiled);
        private static readonly Regex FilterValueRangeRegex = new Regex("RANGE[(](?<LowerBound>.*)[;](?<UpperBound>.*)[)]", RegexOptions.Compiled);
        private static readonly Regex FilterValueInRegex = new Regex("IN[(](?<Values>.*)[)]", RegexOptions.Compiled);
        private static readonly Regex FilterValueFreeTextRegex = new Regex("FREETEXT[(](?<Pattern>.*)[)]", RegexOptions.Compiled);

        private static readonly bool UseBenchmarks = Convert.ToBoolean(CloudConfigurationManager.GetSetting("Benchmarks") ?? false.ToString());


        public readonly Func<IHttpController, JsonApiResource> ReturnResourceGetter;


        public JsonApiAttribute(Type returnResourceType = null)
        {
            if (returnResourceType != null)
            {
                if (!returnResourceType.IsSubclassOf(typeof(JsonApiResource)))
                    throw new ArgumentException("Resource types must inherit from JsonApiResource");

                var instance = (JsonApiResource) Activator.CreateInstance(returnResourceType);
                ReturnResourceGetter = c => instance;
            }
            else
            {
                ReturnResourceGetter = null;
            }
        }
        public JsonApiAttribute(string returnResourceTypeId)
        {
            if (returnResourceTypeId != null)
            {
                ReturnResourceGetter = controller =>
                {
                    if (!(controller is IJsonApiResourceController jsonApiResourceController))
                        throw new Exception($"Controller {controller.GetType()} cannot not return a valid {nameof(JsonApiResource)} because it does not inherit from {nameof(IJsonApiResourceController)}.");

                    var resource = jsonApiResourceController.GetJsonApiResourceById(returnResourceTypeId);
                    if (resource == null)
                        throw new Exception($"Controller {controller.GetType()} does not return a valid {nameof(JsonApiResource)} for the identifier '{returnResourceTypeId}'.");

                    return resource;
                };
            }
            else
            {
                ReturnResourceGetter = null;
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            int? pageNumber = null;
            int? pageSize = null;
            QuerySortProperty[] sort = null;
            string[] includes = null;
            var filter = new List<QueryFilter>();

            var returnResource = ReturnResourceGetter?.Invoke(actionContext.ControllerContext.Controller);
            
            foreach (var queryParam in actionContext.Request.GetQueryNameValuePairs())
            {
                if (queryParam.Key.Equals("page.number"))
                {
                    pageNumber = Convert.ToInt32(queryParam.Value);
                    continue;
                }
                if (queryParam.Key.Equals("page.size"))
                {
                    pageSize = Convert.ToInt32(queryParam.Value);
                    continue;
                }
                if (queryParam.Key.Equals("sort"))
                {
                    sort = queryParam.Value
                        .Split(',')
                        .Select(f => f.StartsWith("-") ? new QuerySortProperty(f.Substring(1), false) : new QuerySortProperty(f, true))
                        .ToArray();
                    continue;
                }
                if (queryParam.Key.Equals("include"))
                {
                    includes = queryParam.Value
                        .Split(',')
                        .ToArray();
                    continue;
                }

                var match = FilterKeyRegex.Match(queryParam.Key);
                if (match.Success)
                {
                    var property = match.Groups["Property"].Value;

                    match = FilterValueRangeRegex.Match(queryParam.Value);
                    if (match.Success)
                    {
                        filter.Add(QueryFilter.Range(property, match.Groups["LowerBound"].Value, match.Groups["UpperBound"].Value));
                        continue;
                    }

                    match = FilterValueInRegex.Match(queryParam.Value);
                    if (match.Success)
                    {
                        filter.Add(QueryFilter.In(property, match.Groups["Values"].Value.Split(';')));
                        continue;
                    }

                    match = FilterValueFreeTextRegex.Match(queryParam.Value);
                    if (match.Success)
                    {
                        filter.Add(QueryFilter.FreeText(property, match.Groups["Pattern"].Value));
                        continue;
                    }

                    filter.Add(QueryFilter.Exact(property, queryParam.Value));
                }
            }
            
            var queryInfo = new QueryInfo(sort, 
                                          filter.Count == 0 ? 
                                              null : 
                                              filter.ToArray(),
                                          pageSize.HasValue ? 
                                              new QueryPaging(pageSize.Value, pageNumber ?? 0) : 
                                              null, 
                                          includes);

            var benchmarks = new BenchmarkCollection(UseBenchmarks);

            actionContext.Request.Properties[nameof(QueryInfo)] = queryInfo;
            actionContext.Request.Properties[nameof(ReturnResourceGetter)] = returnResource;
            actionContext.Request.Properties[nameof(BenchmarkCollection)] = benchmarks;

            if (actionContext.ControllerContext.Controller is IBenchmarkController benchmarkController)
                benchmarkController.Benchmarks = benchmarks;

            base.OnActionExecuting(actionContext);
        }
    }
}