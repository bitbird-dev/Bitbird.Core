using System.Net.Http.Formatting;

namespace Bitbird.Core.WebApi.Net
{
    public class JsonApiConfiguration
    {
        public readonly JsonMediaTypeFormatter Formatter;

        public JsonApiConfiguration(JsonMediaTypeFormatter formatter)
        {
            Formatter = formatter;
        }
    }
}