using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data;
using JetBrains.Annotations;

namespace Bitbird.Core.Api
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