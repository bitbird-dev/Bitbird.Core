using System;
using System.Threading.Tasks;
using Bitbird.Core.Api.Models.Base;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.DbContext;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Query;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Nodes.Core
{
    public interface IInternalReadNode<TService, TSession, TDbContext, TState, TModel, TEntityChangeModel, TEntityTypeId, TId>
        where TModel : ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId>, IId<TId>
        where TService : class, IApiService<TDbContext, TState, TSession, TEntityChangeModel, TEntityTypeId, TId>
        where TDbContext : class, IDisposable, IHookedStateDataContext<TState>
        where TState : IBaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
        where TSession : class, IApiSession
        where TEntityChangeModel : class
    {
        /// <summary>
        /// Queries a single item by its id.
        /// If permission-checks fail, this method does not throw an exception but rather returns null.
        /// If the object does not exist, this method returns null.
        /// Permissions are checked after the query (needed permission: AccessType.Read for the model type and related entities which are needed for model creation).
        /// </summary>
        /// <param name="db">The data context on which to work.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="id">The id to be queried.</param>
        /// <param name="tryCache">Whether to try retrieving the entry from the cache before querying the database.</param>
        /// <param name="addToCache">If tryCache is false and addToCache is true the item is stored in the cache after querying it.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The queried model.</returns>
        [NotNull, ItemCanBeNull]
        Task<TModel> InternalGetByIdAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [CanBeNull] TId id,
            bool tryCache = true,
            bool addToCache = true,
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Queries a items by their id.
        /// If permission-checks fail, this method does not throw an exception but rather returns null-entries.
        /// If objects do not exist, this method returns null-entries.
        /// Permissions are checked after the query (needed permission: AccessType.Read for the model type and related entities which are needed for model creation).
        /// Returns models in the same order as their corresponding id in the ids-parameter.
        /// </summary>
        /// <param name="db">The data context on which to work.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="ids">The ids to be queried.</param>
        /// <param name="tryCache">Whether to try retrieving the entry from the cache before querying the database.</param>
        /// <param name="addToCache">If tryCache is false and addToCache is true the item is stored in the cache after querying it.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The queried models.</returns>
        [NotNull, ItemNotNull]
        Task<TModel[]> InternalGetByIdsAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TId[] ids,
            bool tryCache = true,
            bool addToCache = true,
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Queries data.
        /// If permission-checks fail, this method does not throw an exception but rather returns objects where the permission-check succeeded.
        /// Permissions are checked during the query (needed permission: AccessType.Read for the model type and related entities which are needed for model creation).
        /// </summary>
        /// <param name="db">The data context on which to work.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="queryInfo">Used to specify the query. If this is null the query will not be restricted.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The queried models.</returns>
        [NotNull, ItemNotNull]
        Task<QueryResult<TModel>> InternalGetAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [CanBeNull] QueryInfo queryInfo = null,
            [CanBeNull] BenchmarkSection benchmarks = null);
    }
}