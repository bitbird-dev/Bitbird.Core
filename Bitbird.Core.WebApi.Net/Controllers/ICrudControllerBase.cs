using System;
using JetBrains.Annotations;

namespace Bitbird.Core.WebApi.Net.Controllers
{
    public interface ICrudControllerBase : IReadControllerBase
    {
        bool CanCreate { get; }
        bool CanDelete { get; }
        bool CanUpdate { get; }

        [NotNull] Func<string, bool> CanCreateRelation { get; }
        [NotNull] Func<string, bool> CanUpdateRelation { get; }
        [NotNull] Func<string, bool> CanDeleteRelation { get; }
    }
}