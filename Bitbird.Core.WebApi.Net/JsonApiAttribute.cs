using System;
using System.Collections.Generic;
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

        public readonly JsonApiResource ReturnResource;

        public JsonApiAttribute(Type returnResourceType = null)
        {
            if (returnResourceType != null)
            {
                if (!returnResourceType.IsSubclassOf(typeof(JsonApiResource)))
                    throw new ArgumentException("Resource types must inherit from JsonApiResource");

                ReturnResource = (JsonApiResource)Activator.CreateInstance(returnResourceType);
            }
            else
            {
                ReturnResource = null;
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            int? pageNumber = null;
            int? pageSize = null;
            QuerySortProperty[] sort = null;
            string[] includes = null;
            var filter = new List<QueryFilter>();

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

            QueryPaging paging = null;
            if (pageNumber.HasValue || pageSize.HasValue)
                paging = new QueryPaging(pageSize ?? 20, pageNumber ?? 0);

            if (filter.Count == 0)
                filter = null;

            var queryInfo = new QueryInfo(sort, filter?.ToArray(), paging, includes);

            actionContext.Request.Properties[nameof(QueryInfo)] = queryInfo;
            actionContext.Request.Properties[nameof(ReturnResource)] = ReturnResource;

            var useBenchmarks = Convert.ToBoolean(CloudConfigurationManager.GetSetting("Benchmarks") ?? false.ToString());
            var benchmarks = new BenchmarkCollection(useBenchmarks);

            actionContext.Request.Properties[nameof(BenchmarkCollection)] = benchmarks;


            foreach (var parameter in actionContext.ActionDescriptor.GetParameters())
            {
                if (parameter.ParameterType == typeof(QueryInfo))
                    actionContext.ActionArguments[parameter.ParameterName] = queryInfo;
                if (parameter.ParameterType == typeof(BenchmarkCollection))
                    actionContext.ActionArguments[parameter.ParameterName] = benchmarks;
            }

            if (actionContext.ControllerContext.Controller is IBenchmarkController benchmarkController)
            {
                benchmarkController.Benchmarks = benchmarks;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}