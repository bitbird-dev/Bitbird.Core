using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Query;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.WebApi.Net
{
    public class JsonApiMediaTypeFormatter : MediaTypeFormatter
    {
        public readonly JsonApiConfiguration Config;
        public readonly HttpRequestMessage Request;
        public readonly Dictionary<Type, JsonApiResource> JsonApiResourceMappings;

        public JsonApiMediaTypeFormatter(JsonApiConfiguration config, HttpRequestMessage request = null)
        {
            Config = config;
            Request = request;

            JsonApiResourceMappings = Assembly.GetAssembly(typeof(JsonApiMediaTypeFormatter))
                .GetTypes()
                .Where(t => typeof(JsonApiResource).IsAssignableFrom(t))
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttribute<JsonApiResourceMappingAttribute>()
                })
                .Where(t => t.Attribute != null)
                .ToDictionary(t => t.Attribute.Type, t => (JsonApiResource)Activator.CreateInstance(t.Type));

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.api+json"));
        }

        /// <inheritdoc/>
        public override bool CanReadType(Type type) => true;
        /// <inheritdoc/>
        public override bool CanWriteType(Type type) => false;
        /// <inheritdoc/>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
            => new JsonApiMediaTypeFormatter(Config, request);

        /// <inheritdoc/>
        public override async Task<object> ReadFromStreamAsync(
            Type type,
            Stream readStream,
            HttpContent content,
            IFormatterLogger formatterLogger)
        {
            if (type == typeof(QueryInfo))
                return null;

            using (var reader = new StreamReader(readStream))
            {
                try
                {
                    var json = JToken.Parse(await reader.ReadToEndAsync());
                    var document = json.ToObject<JsonApiDocument>();

                    if (type == typeof(JsonApiDocument))
                        return document;

                    Func<object, JsonApiDocument, Func<string,bool>, object> pack = (o,d,p) => o;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ContentInfo<>))
                    {
                        var contentInfoType = type;
                        pack = (o, d, p) => Activator.CreateInstance(contentInfoType, o, d, p);
                        type = type.GetGenericArguments()[0];
                    }

                    if (!JsonApiResourceMappings.TryGetValue(type, out var resource))
                        throw new Exception($"No mapping from {type.FullName} to a {nameof(JsonApiResource)} was found.");

                    var data = document.ToObject(resource, type, out var foundAttributes);

                    return pack(data, document, foundAttributes);
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(e.ToJsonApiErrorResponseMessage());
                }
            }
        }
    }
}