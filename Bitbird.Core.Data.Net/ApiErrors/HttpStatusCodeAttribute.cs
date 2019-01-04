using System;

namespace Bitbird.Core.Data.Net
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HttpStatusCodeAttribute : Attribute
    {
        public int StatusCode { get; set; }

        public string DefaultMessage { get; set; }
    }
}