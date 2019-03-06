using Bitbird.Core.Json.JsonApi;
using System;

namespace Bitbird.Core.WebApi.Net
{
    public class ContentInfo<T> : IContentInfo
    {
        public readonly T Data;
        public JsonApiDocument Document { get; }
        public Func<string, bool> FoundAttributes { get; }

        public ContentInfo(T data, JsonApiDocument document, Func<string, bool> foundAttributes)
        {
            Data = data;
            Document = document;
            FoundAttributes = foundAttributes;
        }
    }
}