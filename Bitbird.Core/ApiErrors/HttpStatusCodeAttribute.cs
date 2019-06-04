using System;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HttpStatusCodeAttribute : Attribute
    {
        [UsedImplicitly]
        public int StatusCode { get; set; }
    }
}