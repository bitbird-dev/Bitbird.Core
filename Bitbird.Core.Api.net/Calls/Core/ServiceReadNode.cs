using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Bitbird.Core.Api.Net.Core;
using Bitbird.Core.Api.Net.Models.Base;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.Net;
using Bitbird.Core.Data.Net.Cache;
using Bitbird.Core.Data.Net.DbContext;
using Bitbird.Core.Data.Net.DbContext.Hooks;
using Bitbird.Core.Data.Net.Query;
using Bitbird.Core.Query;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Calls.Core
{
    public abstract partial class ServiceReadNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId> 
        : ServiceNodeBase<TService>,
            IInternalReadNode<TService, TSession, TDbContext, TState, TModel, TEntityChangeModel, TEntityTypeId, TId>,
            IServiceReadNode<TSession, TModel, TId>
        where TModel : ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId>, IId<TId>
        where TService : class, IApiService<TDbContext, TState, TSession, TEntityChangeModel, TEntityTypeId, TId>
        where TDbContext : DbContext, IHookedStateDataContext<TState>
        where TState : BaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
        where TSession : class, IApiSession
        where TDbModel : class, IId<TId>
        where TDbMetaData : class, IId<TId>
        where TEntityChangeModel : class
    {
        [NotNull] protected virtual TRightId[] RequiredGetByIdRights { get; } = new TRightId[0];
        [NotNull] protected virtual TRightId[] RequiredGetManyRights { get; } = new TRightId[0];
        [CanBeNull] protected readonly CacheById<TId, TModel> CacheById;
        protected abstract TEntityTypeId EntityType { get; }
        protected virtual bool RaiseChangedEvents { get; } = true;

        protected virtual bool QueryOnMetaData { get; } = false;

        protected ServiceReadNode(
            [NotNull]TService service, 
            bool useCacheById = true, 
            bool expireCacheById = true) 
            : base(service)
        {
            if (useCacheById)
                CacheById = new CacheById<TId, TModel>(service.Redis, $"{GetType().Name}.{nameof(IId<TId>.Id)}", expireCacheById);

            // when primary data gets updated/deleted, clear cache entries.
            Service.DataContextHooks.ForEntity<TDbModel>().AddPostEventAsyncHandler((db, state, entities, type) =>
            {
                if (!RaiseChangedEvents && type == HookEventType.Insert)
                    return Task.CompletedTask;
                
                var ids = entities.Select(e => e.OldEntity.Id != null ? e.OldEntity.Id : e.NewEntity.Id).ToArray();

                if (RaiseChangedEvents)
                    state.PushEntityChangeNotifications(EntityType, ids, type.ToEntityChangeType());

                switch (type)
                {
                    case HookEventType.Insert:
                        break;
                    case HookEventType.Delete:
                    case HookEventType.Update:
                        CacheById?.DeferredDelete(state.RedisDeferred, ids);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                return Task.CompletedTask;
            });
        }

        #region Implementable functions
        /// <summary>
        /// Pre-filter the query for reading objects in any way other than directly by id.
        /// For soft-delete entries this might exclude records that are flagged as deleted.
        /// </summary>
        /// <param name="dbModelQuery">The query to filter. Is not null.</param>
        /// <returns>The filtered query. Must not be null.</returns>
        [NotNull]
        protected abstract IQueryable<TDbModel> FilterDbModelCollection([NotNull] IQueryable<TDbModel> dbModelQuery);
        /// <summary>
        /// Pre-filter the query for reading objects directly by id.
        /// For soft-delete entries this might include the whole set.
        /// </summary>
        /// <param name="dbModelQuery">The query to filter. Is not null.</param>
        /// <returns>The filtered query. Must not be null.</returns>
        [NotNull]
        protected abstract IQueryable<TDbModel> FilterForDirectReadDbModelCollection([NotNull] IQueryable<TDbModel> dbModelQuery);
        /// <summary>
        /// Returns the entity framework data context set on which to work.
        /// </summary>
        /// <param name="db">The data context.</param>
        /// <returns>A set on which to work.</returns>
        [NotNull]
        protected abstract DbSet<TDbModel> GetDbSet([NotNull] TDbContext db);

        /// <summary>
        /// The result must be translatable to SQL.
        /// Must not change the number of elements in the query or the order of elements in the query.
        /// Must not return null.
        /// Is queried from the database when data is requested.
        /// Is used to create the api model.
        /// </summary>
        [NotNull]
        internal abstract IQueryable<TDbMetaData> SelectDbMetaData(
            [NotNull] TDbContext db,
            [NotNull] TSession session,
            [NotNull] IQueryable<TDbModel> query);

        /// <summary>
        /// Creates an api model based on meta data.
        /// </summary>
        /// <param name="dbMetaData">Meta data. Can be null.</param>
        /// <returns>An api model.</returns>
        [NotNull]
        internal abstract TModel DbModelToModel([NotNull] TDbMetaData dbMetaData);

        /// <summary>
        /// Returns a new api error for the given api error.
        /// Deriving classes can override this, if they prefer to specify more details to an api error.
        /// </summary>
        /// <param name="apiError">The api error to replace. Is not null.</param>
        /// <returns>A new api error. Must not be null.</returns>
        [NotNull]
        protected virtual ApiError TranslateApiError([NotNull] ApiError apiError) => apiError;

        #endregion

        #region Read

        /// <inheritdoc />
        public async Task<TModel> InternalGetByIdAsync(
            TDbContext db,
            TSession session,
            TId id,
            bool tryCache = true,
            bool addToCache = true,
            BenchmarkSection benchmarks = null)
        {
            if (id == null)
                return null;

            async Task<TModel> DbTask(TId idParam)
            {
                var dbSet = GetDbSet(db).AsNoTracking();
                var dbDataQuery = FilterForDirectReadDbModelCollection(dbSet);
                dbDataQuery = dbDataQuery.Where(d => d.Id.Equals(idParam)); // TODO: check
                var dataMetaDataQuery = SelectDbMetaData(db, session, dbDataQuery);

                var dbResult = await dataMetaDataQuery.SingleOrDefaultAsync();
                return dbResult == null
                    ? null
                    : DbModelToModel(dbResult);
            }

            TModel model;
            if (CacheById != null)
            {
                if (tryCache)
                    model = await CacheById.GetOrAddAsync(id, DbTask);
                else
                {
                    model = await DbTask(id);

                    if (addToCache)
                        await CacheById.AddOrUpdateAsync(id, model);
                }
            }
            else
                model = await DbTask(id);

            model?.AttachService(Service, session);

            if (model != null && !PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Read, model))
                return null;

            return model;
        }

        /// <inheritdoc />
        public async Task<TModel[]> InternalGetByIdsAsync(
            TDbContext db,
            TSession session,
            TId[] ids,
            bool tryCache = true,
            bool addToCache = true,
            BenchmarkSection benchmarks = null)
        {
            async Task<TModel[]> DbTask(TId[] idParam)
            {
                var mapping = idParam
                    .Select((id, idx) => new
                    {
                        Idx = idx,
                        Id = id
                    })
                    .GroupBy(x => x.Id)
                    .ToDictionary(x => x.Key, x => x.Select(_ => _.Idx).ToArray());
                var queryIds = mapping.Keys.ToArray();

                var dbSet = GetDbSet(db).AsNoTracking();
                var dbDataQuery = FilterForDirectReadDbModelCollection(dbSet);
                dbDataQuery = dbDataQuery.Where(d => queryIds.Contains(d.Id));
                var dataMetaDataQuery = SelectDbMetaData(db, session, dbDataQuery);

                var dbResults = await dataMetaDataQuery.ToArrayAsync();

                var taskModels = new TModel[idParam.Length];
                foreach (var dbResult in dbResults)
                {
                    if (!mapping.TryGetValue(dbResult.Id, out var indices))
                        continue;

                    var model = DbModelToModel(dbResult);

                    foreach (var idx in indices)
                        taskModels[idx] = model;
                }

                return taskModels;
            }

            TModel[] models;
            if (CacheById != null)
            {
                if (tryCache)
                    models = await CacheById.GetOrAddManyAsync(ids, DbTask);
                else
                {
                    models = await DbTask(ids);
                    if (addToCache)
                        await CacheById.AddOrUpdateManyAsync(models.ToDictionary(m => m.Id));
                }
            }
            else
                models = await DbTask(ids);

            for (var i = 0; i < models.Length; i++)
            {
                if (models[i] == null)
                    continue;

                models[i].AttachService(Service, session);

                if (!PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Read, models[i]))
                    models[i] = null;
            }

            return models;
        }


        /// <inheritdoc />
        public async Task<QueryResult<TModel>> InternalGetAsync(
            TDbContext db, 
            TSession session, 
            QueryInfo queryInfo = null,
            BenchmarkSection benchmarks = null)
        {
            var dbSet = GetDbSet(db).AsNoTracking();
            var dbDataQuery = FilterDbModelCollection(dbSet);

            BuiltQuery<TDbMetaData> builtResultsQuery;
            if (QueryOnMetaData)
            {
                builtResultsQuery = SelectDbMetaData(db, session, dbDataQuery.BuildSecuredQuery(session.PermissionResolver))
                    .BuildUnsecuredDbQuery<TDbMetaData, TModel>(queryInfo);
            }
            else
            {
                var builtQuery = dbDataQuery.BuildDbQuery<TDbModel, TModel>(queryInfo, session.PermissionResolver);
                builtResultsQuery = new BuiltQuery<TDbMetaData>(
                    SelectDbMetaData(db, session, builtQuery.Query),
                    SelectDbMetaData(db, session, builtQuery.PagedQuery),
                    builtQuery.PageSize);
            }

            var queryResult = await builtResultsQuery.ExecuteAsync(session.Benchmarks);

            TModel[] models;
            using (session.Benchmarks.CreateBenchmark("Map models"))
            {
                models = queryResult.Data
                    .Select(x =>
                    {
                        var model = DbModelToModel(x);
                        model.AttachService(Service, session);
                        return model;
                    })
                    .ToArray();
            }

            return new QueryResult<TModel>(models, queryResult.RecordCount, queryResult.PageCount);
        }

        #endregion
    }
}