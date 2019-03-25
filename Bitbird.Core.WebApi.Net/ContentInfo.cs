using System;

namespace Bitbird.Core.WebApi.Net
{
    public class ContentInfo<T> : IContentInfo
    {
        public readonly T Data;
        public Func<string, bool> FoundAttributes { get; }

        public ContentInfo(T data, Func<string, bool> foundAttributes)
        {
            Data = data;
            FoundAttributes = foundAttributes;
        }
    }
}