using Bitbird.Core.Json.JsonApi;
using System;

namespace Bitbird.Core.WebApi.Net
{
    public class ContentInfo<T>
    {
        public readonly T Data;
        public readonly JsonApiDocument Document;
        public readonly Func<string, bool> FoundAttributes;

        public ContentInfo(T data, JsonApiDocument document, Func<string, bool> foundAttributes)
        {
            Data = data;
            Document = document;
            FoundAttributes = foundAttributes;
        }
    }
}