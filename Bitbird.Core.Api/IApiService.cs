using System;
using System.Threading.Tasks;
using Bitbird.Core.Api.Core;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.Cache;
using Bitbird.Core.Data.DbContext.Hooks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api
{
    public interface IApiServiceSessionCreator<TSession>
        where TSession : class, IApiSession
    {
        /// <summary>
        /// Creates a session that is assigned to a user.
        /// The session needs to exist.
        /// </summary>
        /// <param name="callData">A <see cref="CallData"/> object that contains information about the user.</param>
        /// <param name="benchmarks">A benchmark section that should be used for benchmark logging. Can be null.</param>
        /// <returns>The system session.</returns>
        [NotNull, ItemNotNull]
        Task<TSession> GetUserSessionAsync([NotNull] CallData callData, [CanBeNull] BenchmarkSection benchmarks);
    }

    public interface IApiService<TDbContext, TState, TSession, TEntityChangeModel, TEntityTypeId, TId>
        : IDisposable
        , IApiServiceSessionCreator<TSession>
        where TDbContext : class
        where TState : IBaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
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