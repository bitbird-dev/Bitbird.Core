using System;
using System.Data.Entity;
using Bitbird.Core.Data.Net.Cache;
using Bitbird.Core.Data.Net.DbContext.Hooks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net
{
    public interface IApiService<TDbContext, TState, in TSession, TEntityChangeModel, TEntityTypeId, TId> : IDisposable
        where TDbContext : DbContext
        where TState : BaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
        where TSession : class, IApiSession
        where TEntityChangeModel : class
    {
        [NotNull]
        TDbContext CreateDbContext([NotNull] TSession apiSession);

        [CanBeNull]
        Redis Redis { get; }

        [NotNull]
        DataContextHookCollection<TDbContext, TState> DataContextHooks { get; }
    }
}