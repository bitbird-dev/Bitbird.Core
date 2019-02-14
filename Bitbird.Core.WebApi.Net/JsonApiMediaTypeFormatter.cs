using System;
using System.Collections;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.WebApi.Net
{
    public class JsonApiMediaTypeFormatter : MediaTypeFormatter
    {
        public readonly JsonApiConfiguration Config;
        public readonly HttpRequestMessage Request;
        public readonly Dictionary<Type, JsonApiResource> JsonApiResourceMappings;

        public JsonApiMediaTypeFormatter(JsonApiConfiguration config, params Assembly[] assemblies)
        {
            Config = config;
            Request = null;

            JsonApiResourceMappings = assemblies
                .GroupBy(a => a.FullName)
                .SelectMany(a => a.First().GetTypes())
                .Where(t => typeof(JsonApiResource).IsAssignableFrom(t))
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttribute<JsonApiResourceMappingAttribute>()
                })
                .Where(t => t.Attribute != null && t.Attribute.IsDefaultDeserializer)
                .ToDictionary(t => t.Attribute.Type, t => (JsonApiResource)Activator.CreateInstance(t.Type));

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.api+json"));
        }

        private JsonApiMediaTypeFormatter(JsonApiConfiguration config, HttpRequestMessage request, Dictionary<Type, JsonApiResource> jsonApiResourceMappings)
        {
            Config = config;
            Request = request;
            JsonApiResourceMappings = jsonApiResourceMappings;
        }

        /// <inheritdoc/>
        public override bool CanReadType(Type type) => true;
        /// <inheritdoc/>
        public override bool CanWriteType(Type type) => false;
        /// <inheritdoc/>
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
            => new JsonApiMediaTypeFormatter(Config, request, JsonApiResourceMappings);


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
                    if (type == typeof(JToken))
                    {
                        using (JsonReader jsonReader = new JsonTextReader(reader))
                        {
                            jsonReader.DateParseHandling = DateParseHandling.None;
                            return await JToken.LoadAsync(jsonReader);
                        }
                    }
                    if (type == typeof(JsonApiCollectionDocument) || typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        JsonApiCollectionDocument document;
                        using (JsonReader jsonReader = new JsonTextReader(reader))
                        {
                            jsonReader.DateParseHandling = DateParseHandling.None;
                            document = (await JToken.LoadAsync(jsonReader)).ToObject<JsonApiCollectionDocument>();
                        }
                        if (type == typeof(JsonApiCollectionDocument))
                            return document;

                        var typeForResource = type;

                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        {
                            typeForResource = type.GetGenericArguments()[0];
                        }

                        if (!JsonApiResourceMappings.TryGetValue(typeForResource, out var resource))
                            throw new Exception($"No (default deserialization) mapping from {typeForResource.FullName} to a {nameof(JsonApiResource)} was found.");

                        return document.ToObject(resource, type, out var foundAttributes);
                    }
                    else
                    {
                        JsonApiDocument document;
                        using (JsonReader jsonReader = new JsonTextReader(reader))
                        {
                            jsonReader.DateParseHandling = DateParseHandling.None;
                            document = (await JToken.LoadAsync(jsonReader)).ToObject<JsonApiDocument>();
                        }
                        if (type == typeof(JsonApiDocument))
                            return document;

                        var typeForResource = type;

                        Func<object, JsonApiDocument, Func<int, string, bool>, object> packContent = (o, d, p) => o;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ContentInfo<>))
                        {
                            var contentInfoType = type;
                            packContent = (o, d, p) => Activator.CreateInstance(contentInfoType, o, d, (Func<string, bool>)(prop => p(0,prop)));
                            type = type.GetGenericArguments()[0];
                            typeForResource = type;
                        }

                        if (!JsonApiResourceMappings.TryGetValue(typeForResource, out var resource))
                            throw new Exception($"No (default deserialization) mapping from {typeForResource.FullName} to a {nameof(JsonApiResource)} was found.");

                        var data = document.ToObject(resource, type, out var foundAttributes);

                        return packContent(data, document, foundAttributes);
                    }
                }
                catch (Exception e)
                {
                    throw new HttpResponseException(e.ToJsonApiErrorResponseMessage());
                }
            }
        }
    }
}