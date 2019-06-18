using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Bitbird.Core.Api;
using Bitbird.Core.Api.Core;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;
using Bitbird.Core.WebApi.JsonApi;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Controllers
{
    [BaseCorsPolicy]
    [JsonApiErrorFilter]
    public abstract class ControllerBase<TService, TSession> : ApiController, IControllerBase
        where TService : class, IApiServiceSessionCreator<TSession>
        where TSession : class, IApiSession
    {
        private const string LoginTokenKeyHeaderKey = "X-ApiKey";
        private const string ClientApplicationHeaderKey = "X-Application";
        private const string InterfaceVersionHeaderKey = "X-InterfaceVersion";
        private const string MobileInterfaceVersionHeaderKey = "X-MobileInterfaceVersion";
        private readonly Regex regexAcceptLanguage = new Regex(@"(?<language>\*|[a-zA-Z]{2})(-(?<country>[a-zA-Z]{2}))?(;q=(?<ratio>\d+\.?\d*))?", RegexOptions.Compiled);

        [NotNull, ItemNotNull]
        [UsedImplicitly]
        // ReSharper disable once StaticMemberInGenericType
        public static HashSet<string> SupportedCultures { get; set; } = new[] { "en" }.ToHashSet();

        [NotNull]
        [UsedImplicitly]
        // ReSharper disable once StaticMemberInGenericType
        public static string DefaultCulture = "en-US";

        [UsedImplicitly]
        // ReSharper disable once StaticMemberInGenericType
        public static long InterfaceVersion = -1;

        [UsedImplicitly]
        // ReSharper disable once StaticMemberInGenericType
        public static long MobileInterfaceVersion = -1;

        [CanBeNull]
        private static TService service;
        [NotNull]
        [UsedImplicitly]
        public static TService Service
        {
            get => service ?? throw new Exception("ControllerBase.Service was not set during startup.");
            set => service = value;
        }

        // injected
        [CanBeNull]
        [UsedImplicitly]
        public BenchmarkCollection Benchmarks { get; set; }

        private TSession session;
        private CallData callData;

        /// <inheritdoc />
        [UsedImplicitly]
        public virtual JsonApiResource GetJsonApiResourceById(string id)
            => throw new NotSupportedException($"{nameof(ControllerBase<TService, TSession>)}.{nameof(GetJsonApiResourceById)}");

        [NotNull, ItemNotNull]
        [UsedImplicitly]
        public async Task<TSession> GetSessionAsync()
        {
            if (session != null)
                return session;

            var callD = GetCallData(Benchmarks);
            return session = await Service.GetUserSessionAsync(callD, Benchmarks);
        }

        private void SetThreadCultures([NotNull] HttpRequestMessage request)
        {
            var acceptLanguageHeaderContents = request.Headers.AcceptLanguage?.Select(x => x.Value).ToArray() ?? new string[0];
            if (acceptLanguageHeaderContents.Length == 0)
                acceptLanguageHeaderContents = new[] { DefaultCulture };

            foreach (var acceptLanguageHeaderContent in acceptLanguageHeaderContents)
            {
                var matches = regexAcceptLanguage.Matches(acceptLanguageHeaderContent);

                var languages = matches
                    .Cast<Match>()
                    .Select(m => new
                    {
                        Language = m.Groups["language"].Value,
                        Country = m.Groups["country"].Success ? m.Groups["country"].Value : null,
                        Ratio = m.Groups["ratio"].Success ? double.Parse(m.Groups["ratio"].Value) : 1.0
                    })
                    .OrderByDescending(x => x.Ratio)
                    .ToArray();

                foreach (var language in languages)
                {
                    if (language.Language.Equals("*", StringComparison.InvariantCulture))
                    {
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(DefaultCulture);
                        return;
                    }

                    if (!SupportedCultures.Contains(language.Language))
                        continue;

                    Thread.CurrentThread.CurrentUICulture = new CultureInfo($"{language.Language}{(language.Country == null ? string.Empty : $"-{language.Country}")}");
                    return;
                }
            }

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(DefaultCulture);
        }

        [UsedImplicitly]
        protected override void Initialize([NotNull] HttpControllerContext controllerContext)
        {
            SetThreadCultures(controllerContext.Request);
            base.Initialize(controllerContext);
        }

        [NonAction]
        [UsedImplicitly]
        protected CallData GetCallData([CanBeNull] BenchmarkSection benchmarks = null)
        {
            if (callData != null)
                return callData;

            using (benchmarks.CreateBenchmark("ReadCallData"))
            {
                var loginTokenKey = string.Empty;
                string clientApplicationId = null;

                if (Request?.Headers != null)
                {
                    if (Request.Headers.TryGetValues(LoginTokenKeyHeaderKey, out var valuesLoginToken))
                        loginTokenKey = valuesLoginToken?.FirstOrDefault();

                    if (Request.Headers.TryGetValues(ClientApplicationHeaderKey, out var valuesClientApplicationId))
                        clientApplicationId = valuesClientApplicationId?.FirstOrDefault();

                    if (Request.Headers.TryGetValues(InterfaceVersionHeaderKey, out var interfaceVersionString))
                    {
                        var versionStrings = interfaceVersionString as string[] ?? interfaceVersionString?.ToArray();

                        if (versionStrings == null)
                            throw new ApiErrorException(new ApiParameterError(InterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (!versionStrings.Any())
                            throw new ApiErrorException(new ApiParameterError(InterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (versionStrings.Any(x => x == null))
                            throw new ApiErrorException(new ApiParameterError(InterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (versionStrings.Length != 1)
                            throw new ApiErrorException(new ApiParameterError(InterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (!long.TryParse(versionStrings.FirstOrDefault() ?? string.Empty, out var interfaceVersion))
                            throw new ApiErrorException(new ApiParameterError(InterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));

                        if (interfaceVersion != InterfaceVersion)
                            throw new ApiErrorException(new ApiVersionMismatchError(InterfaceVersion, interfaceVersion));
                    }

                    if (Request.Headers.TryGetValues(MobileInterfaceVersionHeaderKey, out var mobileInterfaceVersionString))
                    {
                        var versionStrings = mobileInterfaceVersionString as string[] ?? mobileInterfaceVersionString?.ToArray();

                        if (versionStrings == null)
                            throw new ApiErrorException(new ApiParameterError(MobileInterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (!versionStrings.Any())
                            throw new ApiErrorException(new ApiParameterError(MobileInterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (versionStrings.Any(x => x == null))
                            throw new ApiErrorException(new ApiParameterError(MobileInterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (versionStrings.Length != 1)
                            throw new ApiErrorException(new ApiParameterError(MobileInterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));
                        if (!long.TryParse(versionStrings.FirstOrDefault() ?? string.Empty, out var mobileInterfaceVersion))
                            throw new ApiErrorException(new ApiParameterError(MobileInterfaceVersionHeaderKey, Properties.Resources.ControllerBase_InferfaceVersionHeader_WrongFormat));

                        if (mobileInterfaceVersion != MobileInterfaceVersion)
                            throw new ApiErrorException(new ApiVersionMismatchError(MobileInterfaceVersion, mobileInterfaceVersion));
                    }
                }

                callData = new CallData(new ApiSessionData(loginTokenKey, clientApplicationId), Request?.RequestUri);

                return callData;
            }
        }

        /// <summary>
        /// Adds includes to the currently active <see cref="QueryInfo"/> of this request.
        /// </summary>
        /// <param name="includes">The includes to add.</param>
        [UsedImplicitly]
        protected void AddQueryInfoIncludes([NotNull, ItemNotNull] params string[] includes)
        {
            var currentQueryInfo = QueryInfo;
            QueryInfo = new QueryInfo(currentQueryInfo?.SortProperties, 
                currentQueryInfo?.Filters, 
                currentQueryInfo?.Paging,
                (currentQueryInfo?.Includes ?? new string[0]).Union(includes).ToArray());
        }

        [CanBeNull]
        private QueryInfo queryInfo;
        [CanBeNull]
        [UsedImplicitly]
        protected QueryInfo QueryInfo
        {
            get
            {
                if (queryInfo != null)
                    return queryInfo;

                if (Request.Properties.TryGetValue(nameof(QueryInfo), out var untypedQueryInfo) && untypedQueryInfo is QueryInfo typedQueryInfo)
                    return queryInfo = typedQueryInfo;
                
                queryInfo = new QueryInfo();
                Request.Properties[nameof(QueryInfo)] = queryInfo;
                return queryInfo;
            }
            set
            {
                queryInfo = value;
                Request.Properties[nameof(QueryInfo)] = queryInfo;
            }
        }
    }
}