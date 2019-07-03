using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Query;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Bitbird.Core.Web.AzureFunctions.V2.NetCore
{
    [UsedImplicitly]
    public class JsonApiOutputFormatter : TextOutputFormatter
    {
        [NotNull] private readonly JsonSerializerSettings JsonSerializerSettings;

        public JsonApiOutputFormatter([NotNull] JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings ?? throw new ArgumentNullException(nameof(jsonSerializerSettings));

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add("application/vnd.api+json");

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

            if (!string.Equals(context.ContentType.Value, "application/vnd.api+json", StringComparison.InvariantCultureIgnoreCase))
                throw new Exception($"Content-type '{context.ContentType.Value}' not supported.");

            return WriteJsonApiOutputAsync(context.HttpContext.Response, selectedEncoding, model.Data, model.DataType, model.Resource, model.QueryInfo);
        }

        [NotNull]
        private async Task WriteJsonApiOutputAsync([NotNull] HttpResponse context,
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

            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            using (var streamWriter = new StreamWriter(context.Body, selectedEncoding))
            {
                serializer.Serialize(streamWriter, modelData, modelDataType);
                await streamWriter.FlushAsync();
            }
        }
    }
}