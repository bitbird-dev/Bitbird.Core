using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Core
{
    public class CallData
    {
        [NotNull] public readonly ApiSessionData ApiSessionData;
        [CanBeNull] public readonly Uri RequestUri;

        public CallData(
            [NotNull] ApiSessionData apiSessionData, 
            [CanBeNull] Uri requestUri)
        {
            ApiSessionData = apiSessionData;
            RequestUri = requestUri;
        }
    }
}