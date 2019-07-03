using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Query;
using Bitbird.Core.Web.JsonApi;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Bitbird.Core.Web.AzureFunctions.V2.NetCore
{
    [UsedImplicitly]
    public class JsonOutputFormatter : TextOutputFormatter
    {
        [NotNull] private readonly JsonSerializerSettings JsonSerializerSettings;

        public JsonOutputFormatter([NotNull] JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add("application/json");

            SupportedEncodings.Clear();
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedEncodings.Add(Encoding.UTF32);
            SupportedEncodings.Add(Encoding.UTF7);
            SupportedEncodings.Add(Encoding.ASCII);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IFunctionResultContent).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var model = context.Object as IFunctionResultContent
                        ?? throw new Exception($"Expected type {nameof(IFunctionResultContent)}, but found {context.Object?.GetType().Name}.");

            if (!string.Equals(context.ContentType.Value, "application/json", StringComparison.InvariantCultureIgnoreCase))
                throw new Exception($"Content-type '{context.ContentType.Value}' not supported.");

            return WriteJsonOutputAsync(context.HttpContext.Response, selectedEncoding, model.Data, model.DataType, model.Resource, model.QueryInfo);
        }

        [NotNull]
        private async Task WriteJsonOutputAsync(
            [NotNull] HttpResponse context,
            [NotNull] Encoding selectedEncoding,
            [CanBeNull] object modelData,
            [NotNull] Type modelDataType,
            [NotNull] JsonApiResource modelResource,
            [CanBeNull] QueryInfo queryInfo)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (selectedEncoding == null) throw new ArgumentNullException(nameof(selectedEncoding));
            if (modelDataType == null) throw new ArgumentNullException(nameof(modelDataType));
            if (modelResource == null) throw new ArgumentNullException(nameof(modelResource));

            var meta = new JsonApiMetaData();

            if (modelData is IQueryResult queryResult)
            {
                meta.PageCount = queryResult.PageCount;
                meta.RecordCount = queryResult.RecordCount;
                modelData = queryResult.Data;
            }

            IJsonApiDocument document;
            if (modelData is IEnumerable collectionValue)
            {
                var apiCollectionDocument = new JsonApiCollectionDocument();
                apiCollectionDocument.FromApiResource(collectionValue, modelResource);
                document = apiCollectionDocument;
            }
            else
            {
                var apiDocument = new JsonApiDocument();
                apiDocument.FromApiResource(modelData, modelResource);
                document = apiDocument;
            }

            document.Meta = meta;

            if (queryInfo?.Includes != null)
                foreach (var include in queryInfo.Includes)
                    document.IncludeRelation(modelResource, modelData, include);

            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            using (var streamWriter = new StreamWriter(context.Body, selectedEncoding))
            {
                serializer.Serialize(streamWriter, document, document.GetType());
                await streamWriter.FlushAsync();
            }
        }
    }
}