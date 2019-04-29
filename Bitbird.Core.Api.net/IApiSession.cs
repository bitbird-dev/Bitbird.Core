using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.Net;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net
{
    public interface IApiSession
    {
        [CanBeNull]
        IPermissionResolver PermissionResolver { get; }

        [CanBeNull]
        BenchmarkSection Benchmarks { get; }

        bool IsSystemSession { get; }
        bool IsUserLoggedIn { get; }
        bool IsSystemSessionOrUserLoggedIn { get; }
    }
}