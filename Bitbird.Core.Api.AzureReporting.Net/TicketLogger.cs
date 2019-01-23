using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bitbird.Core.Api.AzureReporting.Net.Core;
using Bitbird.Core.Api.AzureReporting.Net.Models;
using Microsoft.Azure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Bitbird.Core.Api.AzureReporting.Net
{
    /// <summary>
    /// A static helper class that provides methods to log/query tickets to Azure DevOps.
    /// </summary>
    public static class TicketLogging
    {
        /// <summary>
        /// The api-version of the Azure DevOps REST interface that we address.
        /// </summary>
        private const string ApiVersion = "4.1";
        /// <summary>
        /// Settings for de- & serialization of data during REST requests with the Azure DevOps REST interface.
        /// </summary>
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };


        /// <summary>
        /// Creates a ticket in Azure DevOps.
        /// </summary>
        /// <param name="createTicketModel">The ticket to create.</param>
        /// <returns>A TicketModel that describes the created ticket.</returns>
        public static async Task<TicketModel> CreateTicketAsync(ICreateTicketModel createTicketModel)
        {
            if (createTicketModel == null)
                throw new ArgumentNullException(nameof(createTicketModel), "The passed createTicketModel must not be null.");

            var areaPath = CloudConfigurationManager.GetSetting("AzureReporting.AreaPath") ?? throw new Exception("Configuration: AzureReporting.AreaPath must not be empty.");

            using (var httpClient = CreateClient())
            {
                httpClient.SetBaseUri("wit");

                var patchOperations = createTicketModel.ToPatchOperations().Concat(new[]
                {
                    new PatchOperation("add", "/fields/System.AreaPath", areaPath)
                });
                var result = JToken.Parse(await httpClient.SendRequestAsync($"workitems{createTicketModel.GetUriPath()}", null, "PATCH", patchOperations, "application/json-patch+json"));

                return TicketModel.FromJToken(result);
            }
        }
        /// <summary>
        /// Attaches files to an existing ticket in Azure DevOps.
        /// </summary>
        /// <param name="ticketId">The id of the ticket.</param>
        /// <param name="files">A list of files to attach. Must not be null or empty. Must not contain null.</param>
        /// <returns>Nothing</returns>
        public static async Task AttachFileToTicketAsync(long ticketId, IUploadAttachmentModel[] files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files), "The passed file collection must not be null.");
            if (!files.Any())
                throw new ArgumentNullException(nameof(files), "The passed file collection must not be empty.");
            if (files.Any(f => f == null))
                throw new ArgumentNullException(nameof(files), "The passed file collection must contain null.");
            if (files.Any(f => f.Filename == null))
                throw new ArgumentNullException(nameof(files), "The passed filename must not be null.");
            if (files.Any(f => f.GetStream == null))
                throw new ArgumentNullException(nameof(files), "The passed file-data-handler must not be null.");
            
            using (var httpClient = CreateClient())
            {
                httpClient.SetBaseUri("wit");

                var urls = new List<string>();
                foreach (var file in files)
                {
                    var queryParams = $"fileName={Uri.EscapeDataString(file.Filename)}";
                    var result = JToken.Parse(await httpClient.SendRequestAsync("attachments", queryParams, "POST", file.GetStream(), "application/octet-stream"));

                    urls.Add(result["url"].Value<string>());
                }

                var patchOperations = urls.Select(url => new PatchOperation("add", "/relations/-", new
                {
                    Rel = "AttachedFile",
                    Url = url
                })).ToArray();

                await httpClient.SendRequestAsync($"workitems/{ticketId}", null, "PATCH", patchOperations, "application/json-patch+json");
            }
        }

        /// <summary>
        /// Queries tickets from Azure DevOps based on a configured Query.
        /// </summary>
        /// <returns>TicketModels representing the tickets-results of the Query.</returns>
        public static async Task<TicketModel[]> QueryTicketsAsync()
        {
            var queryId = CloudConfigurationManager.GetSetting("AzureReporting.QueryId") ?? throw new Exception("Configuration: AzureReporting.QueryId must not be empty.");

            using (var httpClient = CreateClient())
            {
                httpClient.SetBaseUri("wit");

                var result = JToken.Parse(await httpClient.SendRequestAsync($"wiql/{queryId}"));
                var ids = result["workItems"].Children().Select(workItem => workItem["id"].Value<long>()).ToArray();

                if (!ids.Any())
                    return new TicketModel[0];

                var queryParams = $"ids={string.Join(",", ids)}&fields={string.Join(",", "System.Title", "System.WorkItemType", "System.State")}&$expand=links";
                result = JToken.Parse(await httpClient.SendRequestAsync("workitems", queryParams));

                return result["value"].Children().Select(TicketModel.FromJToken).ToArray();
            }
        }

        /// <summary>
        /// Creates a HttpClient that supports authentication. The result must be disposed by the caller.
        /// </summary>
        /// <returns>A HttpClient.</returns>
        private static HttpClient CreateClient()
        {
            var personalAccessToken = CloudConfigurationManager.GetSetting("AzureReporting.AccessToken") ?? throw new Exception("Configuration: AzureReporting.AccessToken must not be empty.");

            const string username = "";

            var httpMessageHandler = new HttpClientHandler();
            HttpClient httpClient = null;
            try
            {
                httpClient = new HttpClient(httpMessageHandler, true);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{personalAccessToken}")));

                return httpClient;
            }
            catch
            {
                if (httpClient != null)
                {
                    try
                    {
                        httpClient.Dispose();
                        httpMessageHandler = null;
                    }
                    catch
                    {
                        /* ignored */
                    }
                }

                try
                {
                    httpMessageHandler?.Dispose();
                }
                catch
                {
                    /* ignored */
                }

                throw;
            }
        }

        /// <summary>
        /// Sets the base-address of the HttpClient to "https://{instance}/{teamProject}/_apis/{area}/".
        /// </summary>
        /// <param name="httpClient">The HttpClient to set the base-address on. Must not be null.</param>
        /// <param name="area">The area used for the uri. Must not be null.</param>
        private static void SetBaseUri(this HttpClient httpClient, string area)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient), "The passed httpClient must not be null.");
            if (area == null)
                throw new ArgumentNullException(nameof(area), "The passed area must not be null.");

            var instance = CloudConfigurationManager.GetSetting("AzureReporting.Instance") ?? throw new Exception("Configuration: AzureReporting.Instance must not be empty.");
            var teamProject = CloudConfigurationManager.GetSetting("AzureReporting.TeamProject") ?? throw new Exception("Configuration: AzureReporting.TeamProject must not be empty.");

            httpClient.BaseAddress = new Uri($"https://{instance}/{teamProject}/_apis/{area}/");
        }

        /// <summary>
        /// Sends a http-request to Azure DevOps.
        ///
        /// If a newer Http-Operation(Verb) is found, a fallback to X-HTTP-Method-Override is implemented.
        ///
        /// Queries the path "{resource}?api-version={ApiVersion}" and adds the given query-parameters.
        ///
        /// If the content-type is "application/octet-stream", the body is expected to be of type Steam.
        /// If body is null, no content will be sent.
        /// If body is of type JToken, the JToken will be sent as string.
        /// Otherwise body will be serialized using JsonConvert.
        ///
        /// If an error occurs during the request/response an exception is raised.
        /// If no error occurs, the response will be returned as string. 
        /// </summary>
        /// <param name="httpClient">The HttpClient to be used for the transfer. Must not be null.</param>
        /// <param name="resource">The resource to be used for the path. See summary. Must not be null or empty.</param>
        /// <param name="queryParams">The queryParams to be used for the path. Can be null or empty.</param>
        /// <param name="operation">The Http-operation to be used. Must not be null or empty. Must contain a valid Http-operation.</param>
        /// <param name="body">The body to be sent. See summary. Can be null.</param>
        /// <param name="contentType">The content-type of the request-body. Ignored if body is null. See summary.</param>
        /// <returns>The response content as string, if no error occurred.</returns>
        private static async Task<string> SendRequestAsync(this HttpClient httpClient, string resource, string queryParams = null, string operation = "GET", object body = null, string contentType = "application/json")
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient), "The passed httpClient must not be null.");
            if (resource == null)
                throw new ArgumentNullException(nameof(resource), "The passed resource must not be null.");
            if (operation == null)
                throw new ArgumentNullException(nameof(operation), "The operation area must not be null.");

            var path = $"{resource}?api-version={ApiVersion}{(string.IsNullOrWhiteSpace(queryParams) ? string.Empty : $"&{queryParams}")}";

            string overrideMethodHeader = null;
            if (operation.Equals("PATCH"))
            {
                overrideMethodHeader = operation;
                operation = "POST";
            }

            using (var request = new HttpRequestMessage(new HttpMethod(operation), path))
            {
                if (overrideMethodHeader != null)
                    request.Headers.Add("X-HTTP-Method-Override", overrideMethodHeader);

                if (contentType == "application/octet-stream" && body is Stream stream)
                {
                    request.Content = new StreamContent(stream);
                }
                else if (body != null)
                {
                    string bodyContent;
                    if (body is JToken bodyToken)
                        bodyContent = bodyToken.ToString();
                    else
                        bodyContent = JsonConvert.SerializeObject(body, JsonSerializerSettings);

                    request.Content = new StringContent(bodyContent, Encoding.UTF8, contentType);
                }

                var response = await httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();;

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"The server returned an error: {response.StatusCode} ({response.ReasonPhrase}).\nDetails:\n{responseContent}");

                return responseContent;
            }
        }
    }
}