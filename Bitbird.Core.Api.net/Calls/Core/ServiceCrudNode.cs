using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bitbird.Core.Api.Net.Models.Base;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.Net;
using Bitbird.Core.Data.Net.DbContext;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Calls.Core
{
    public abstract partial class ServiceCrudNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId> 
        : ServiceReadNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
            , IServiceCrudNode<TSession, TModel, TId> 
            , IInternalCrudNode<TService, TSession, TDbContext, TState, TModel, TEntityChangeModel, TEntityTypeId, TId>
        where TModel : ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId>, IId<TId>
        where TService : class, IApiService<TDbContext, TState, TSession, TEntityChangeModel, TEntityTypeId, TId>
        where TDbContext : DbContext, IHookedStateDataContext<TState>
        where TState : BaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
        where TSession : class, IApiSession
        where TEntityChangeModel : class
        where TDbModel : class, IId<TId>
        where TDbMetaData : class, IId<TId>
    {
        protected virtual TRightId[] RequiredUpdateRights { get; } = new TRightId[0];
        protected virtual TRightId[] RequiredCreateRights { get; } = new TRightId[0];
        protected virtual TRightId[] RequiredCreateManyRights { get; } = new TRightId[0];
        protected virtual TRightId[] RequiredDeleteRights { get; } = new TRightId[0];

        protected ServiceCrudNode(
            [NotNull]TService service,
            bool useCacheById = true,
            bool expireCacheById = true)
            : base(service, useCacheById, expireCacheById)
        {
        }

        #region Implementable functions 

        /// <summary>
        /// Creates a new db model instance based on an api model.
        /// May also create related records.
        /// </summary>
        /// <param name="db">The data context on which to work.</param>
        /// <param name="session"></param>
        /// <param name="model">The model to create.</param>
        /// <returns>The created db model.</returns>
        [NotNull, ItemNotNull]
        protected abstract Task<TDbModel> CreateDbModel([NotNull] TDbContext db,
            TSession session,
            [NotNull] TModel model);

        /// <summary>
        /// Updates the db model based on an api model.
        /// May also create/delete related records.
        /// </summary>
        /// <param name="db">The data context on which to work.</param>
        /// <param name="session"></param>
        /// <param name="dbModel">The model to update.</param>
        /// <param name="model">The source data for the update.</param>
        /// <param name="updatedProperties">A predicate that evaluates to true for properties that should be updated. Can be null.</param>
        /// <returns></returns>
        [NotNull, ItemNotNull]
        protected abstract Task<TDbModel> UpdateDbModel([NotNull] TDbContext db,
            TSession session,
            [NotNull] TDbModel dbModel,
            [NotNull] TModel model,
            [CanBeNull] Func<string, bool> updatedProperties);

        /// <summary>
        /// Deletes a db model from the database.
        /// May also delete related records.
        /// For soft deletes, this might only set the <see cref="IIsDeletedFlagEntity.IsDeleted"/> flat to true.
        /// </summary>
        /// <param name="db">The data context on which to work.</param>
        /// <param name="session">The current session.</param>
        /// <param name="dbModel">The model to delete.</param>
        /// <returns></returns>
        [NotNull]
        protected abstract Task DeleteDbModel(
            [NotNull] TDbContext db,
            [NotNull] TSession session,
            [NotNull] TDbModel dbModel);

        /// <summary>
        /// Returns a new exception for a given <see cref="DbUpdateException"/>.
        /// Deriving classes can override this, if they prefer to specify more details to an db update exception (e.g. they detect that an index was violated and want to return a specific error).
        /// Strongly consider returning an <see cref="ApiErrorException"/>.
        /// </summary>
        /// <param name="exc">The exception to replace. Is not null.</param>
        /// <returns>The new exception. Must not be null.</returns>
        [NotNull]
        protected virtual Exception TranslateDbUpdateException([NotNull] DbUpdateException exc) => exc;

        #endregion

        #region Exception Handling

        /// <summary>
        /// Scans the passed exception for structured sql errors. If such an error was found the function returns an ApiErrorException with the detailed info.
        /// Otherwise TranslateDbUpdateException (implementable by deriving classes) is called.
        /// </summary>
        /// <param name="exc">The exception to scan.</param>
        /// <returns>The translated exception.</returns>
        [NotNull]
        protected Exception TranslateDbUpdateExceptionCore([NotNull] DbUpdateException exc)
        {
            // Currently no structured sql error is used in any implementation.
            /*if (exc.TryGetInnerException<SqlException>(out var sqlException))
            {
                StructuredSqlError error = null;
                try
                {
                    error = JsonConvert.DeserializeObject<StructuredSqlError>(sqlException.Message);
                }
                catch
                {
                    // ignored
                }
            }*/

            return TranslateDbUpdateException(exc);
        }
        /// <summary>
        /// Extracts ApiAttributeErrors if suitable information was found, otherwise returns a generic ApiEntityError. Returns an ApiErrorException containing all found errors.
        /// </summary>
        /// <param name="exc">The exception to scan.</param>
        /// <returns>An ApiErrorException containing detailed info.</returns>
        [NotNull]
        private ApiErrorException TranslateDbEntityValidationException([NotNull] DbEntityValidationException exc)
        {
            return new ApiErrorException(exc, exc.EntityValidationErrors.SelectMany(entityError => entityError.ValidationErrors.Select(validationError =>
            {
                // TODO: map to ApiModel
                try
                {
                    var property = typeof(TDbModel).GetProperty(validationError.PropertyName);
                    if (property != null)
                    {
                        var parameter = Expression.Parameter(typeof(TDbModel));
                        var memberExpression = Expression.Property(parameter, validationError.PropertyName);
                        var lambdaExpression = Expression.Lambda(memberExpression, parameter);

                        var apiParameterErrorType = typeof(ApiAttributeError<,>).MakeGenericType(typeof(TDbModel), property.PropertyType);

                        return (ApiError)Activator.CreateInstance(apiParameterErrorType, lambdaExpression, validationError.ErrorMessage);
                    }
                }
                catch
                {
                    /* fallback to generic error */
                }

                return new ApiEntityError<TDbModel>(validationError.ErrorMessage);
            })).Select(TranslateApiError).ToArray());
        }

        #endregion

        #region CRUD

        #region Create

        /// <inheritdoc />
        public async Task<TModel> InternalCreateAsync(
            TDbContext db,
            TSession session,
            TState state,
            TModel model,
            BenchmarkSection benchmarks)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            using (var benchmarkSection = benchmarks.CreateSection($"{nameof(InternalCreateAsync)}<{typeof(TModel).Name}>"))
            {
                using (benchmarkSection.CreateBenchmark("Permission check"))
                {
                    if (!PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Create, model))
                        throw new ApiErrorException(TranslateApiError(new ApiForbiddenNoRightsError($"create {typeof(TModel).Name}")));
                }

                TDbModel dbModel;
                using (benchmarkSection.CreateBenchmark("CreateDbModel"))
                {
                    dbModel = await CreateDbModel(db, session, model);
                }

                using (benchmarkSection.CreateBenchmark("SaveChangesAsync"))
                {
                    try
                    {
                        await db.SaveChangesAsync(state);
                    }
                    catch (DbUpdateException e)
                    {
                        throw TranslateDbUpdateExceptionCore(e);
                    }
                    catch (DbEntityValidationException e)
                    {
                        throw TranslateDbEntityValidationException(e);
                    }
                }

                using (benchmarkSection.CreateBenchmark("InternalGetByIdAsync"))
                {
                    model = (await InternalGetByIdAsync(db, session, dbModel.Id, false, false, benchmarkSection))
                            ?? throw new Exception($"Could not find element by id after creation (Entity = {typeof(TModel).Name}, Id = {dbModel.Id}).");
                }

                return model;
            }
        }

        /// <inheritdoc />
        public async Task<TModel[]> InternalBatchCreateAsync(
            TDbContext db,
            TSession session,
            TState state,
            TModel[] models,
            BenchmarkSection benchmarks)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            using (var benchmarkSection = benchmarks.CreateSection($"{nameof(InternalBatchCreateAsync)}<{typeof(TModel).Name}>"))
            {
                using (benchmarkSection.CreateBenchmark("Permission check"))
                {
                    if (models.Any(model => !PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Create, model)))
                        throw new ApiErrorException(TranslateApiError(new ApiForbiddenNoRightsError($"create {typeof(TModel).Name}")));
                }

                TDbModel[] dbModels;
                using (benchmarkSection.CreateBenchmark("CreateDbModel"))
                {
                    dbModels = new TDbModel[models.Length];
                    for (var i = 0; i < models.Length; i++)
                        dbModels[i] = await CreateDbModel(db, session, models[i]);
                }

                const int batchSize = 500;

                using (benchmarkSection.CreateBenchmark("AddRange,SaveChangesAsync"))
                {
                    var batchCount = dbModels.Length / batchSize + ((dbModels.Length % batchSize) == 0 ? 0 : 1);
                    for (var i = 0; i < batchCount; i++)
                    {
                        GetDbSet(db).AddRange(dbModels.Skip(batchSize * i).Take(batchSize));

                        try
                        {
                            await db.SaveChangesAsync(state);
                        }
                        catch (DbUpdateException e)
                        {
                            throw TranslateDbUpdateExceptionCore(e);
                        }
                        catch (DbEntityValidationException e)
                        {
                            throw TranslateDbEntityValidationException(e);
                        }
                    }
                }

                using (benchmarkSection.CreateBenchmark("InternalGetByIdsAsync"))
                {
                    models = await InternalGetByIdsAsync(db, session, dbModels.Select(dbModel => dbModel.Id).ToArray(),  false, false, benchmarkSection);
                    if (models.Any(m => m == null))
                        throw new Exception($"Could not find element by id after batch creation (Entity = {typeof(TModel).Name}).");
                }

                return models;
            }
        }

        #endregion

        #region Update

        /// <inheritdoc />
        public async Task<TModel> InternalUpdateAsync(
            TDbContext db,
            TSession session,
            TState state,
            TModel model,
            Func<string, bool> updatedProperties,
            BenchmarkSection benchmarks)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            
            using (var benchmarkSection = benchmarks.CreateSection($"{nameof(InternalUpdateAsync)}<{typeof(TModel).Name}>"))
            {
                var id = model.Id;

                TDbModel currentDbModel;
                TModel currentModel;
                using (benchmarkSection.CreateBenchmark("Query Current Models"))
                {
                    var dbModelQuery = FilterDbModelCollection(GetDbSet(db))
                        .Where(d => d.Id.Equals(id));
                    var dbMetaDataQuery = SelectDbMetaData(db, session, dbModelQuery);

                    var currentModelData = (await dbModelQuery
                                                .Join(dbMetaDataQuery,
                                                    dbModel => dbModel.Id,
                                                    dbModelMetaData => dbModelMetaData.Id,
                                                    (dbModel, dbModelMetaData) => new
                                                    {
                                                        DbModel = dbModel,
                                                        DbModelMetaData = dbModelMetaData
                                                    })
                                                .SingleOrDefaultAsync())
                                           ?? throw new ApiErrorException(TranslateApiError(ApiNotFoundError.Create(typeof(TModel).Name, model.Id)));

                    currentDbModel = currentModelData.DbModel;
                    currentModel = DbModelToModel(currentModelData.DbModelMetaData);
                    currentModel.AttachService(Service, session);
                }

                using (benchmarkSection.CreateBenchmark("Permission check"))
                {
                    if (!PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Update, currentModel))
                        throw new ApiErrorException(
                            TranslateApiError(new ApiForbiddenNoRightsError($"update {typeof(TModel).Name}")));
                }

                using (benchmarkSection.CreateBenchmark("UpdateDbModel"))
                {
                    var updatedDbModel = await UpdateDbModel(db, session, currentDbModel, model, updatedProperties);

                    if (updatedDbModel is IOptimisticLockable lockableDbModel)
                        db.Entry(updatedDbModel).OriginalValues[nameof(IOptimisticLockable.SysStartTime)] = lockableDbModel.SysStartTime;
                }

                using (benchmarkSection.CreateBenchmark("SaveChangesAsync"))
                {
                    try
                    {
                        await db.SaveChangesAsync(state);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw new ApiErrorException(
                            new ApiOptimisticLockingError<TModel>($"The model {typeof(TModel).Name} has changed."));
                    }
                    catch (DbUpdateException e)
                    {
                        throw TranslateDbUpdateExceptionCore(e);
                    }
                    catch (DbEntityValidationException e)
                    {
                        throw TranslateDbEntityValidationException(e);
                    }
                }

                using (benchmarkSection.CreateBenchmark("InternalGetByIdAsync"))
                {
                    model = (await InternalGetByIdAsync(db, session, id, false, false, benchmarkSection))
                            ?? throw new Exception($"Could not find element by id after update (Entity = {typeof(TModel).Name}, Id = {id}).");
                }

                return model;
            }
        }

        /// <inheritdoc />
        public async Task<TModel[]> InternalBatchUpdateAsync(
            TDbContext db,
            TSession session,
            TState state,
            TModel[] models,
            Func<string, bool>[] updatedProperties,
            BenchmarkSection benchmarks)
        {
            if (models == null || models.Any(model => model == null))
                throw new ArgumentNullException(nameof(models));
            if (updatedProperties == null)
                throw new ArgumentNullException(nameof(updatedProperties));
            if (models.GroupBy(model => model.Id).Any(group => group.Count() != 1))
                throw new InvalidOperationException("Cannot update the same element twice in one batch.");

            if (models.Length == 0)
                return new TModel[0];

            using (var benchmarkSection = benchmarks.CreateSection($"{nameof(InternalBatchUpdateAsync)}<{typeof(TModel).Name}>"))
            {
                var mapping = models
                .Select((model, idx) => new
                {
                    Idx = idx,
                    model.Id
                })
                .ToDictionary(x => x.Id, x => x.Idx);

                var ids = mapping.Keys.ToArray();

                var currentDbModels = new TDbModel[ids.Length];
                var currentModels = new TModel[ids.Length];
                using (benchmarkSection.CreateBenchmark("Query Current Models"))
                {
                    var dbModelQuery = FilterDbModelCollection(GetDbSet(db))
                        .Where(d => ids.Contains(d.Id));
                    var dbMetaDataQuery = SelectDbMetaData(db, session, dbModelQuery);

                    var currentModelDataById = await dbModelQuery
                                               .Join(dbMetaDataQuery,
                                                   dbModel => dbModel.Id,
                                                   dbModelMetaData => dbModelMetaData.Id,
                                                   (dbModel, dbModelMetaData) => new
                                                   {
                                                       DbModel = dbModel,
                                                       DbModelMetaData = dbModelMetaData
                                                   })
                                               .ToDictionaryAsync(x => x.DbModel.Id);

                    if (ids.Length != currentModelDataById.Count)
                    {
                        throw new ApiErrorException(ids
                            .Where(id => !currentModelDataById.ContainsKey(id))
                            .Select(id => TranslateApiError(ApiNotFoundError.Create(typeof(TModel).Name, id)))
                            .ToArray());
                    }

                    foreach (var currentModelData in currentModelDataById.Values)
                    {
                        if (!mapping.TryGetValue(currentModelData.DbModel.Id, out var index))
                            continue;

                        currentDbModels[index] = currentModelData.DbModel;
                        var model = DbModelToModel(currentModelData.DbModelMetaData);
                        model.AttachService(Service, session);
                        currentModels[index] = model; 
                    }
                }

                using (benchmarkSection.CreateBenchmark("Permission check"))
                {
                    if (currentModels.Any(currentModel => !PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Update, currentModel)))
                        throw new ApiErrorException(TranslateApiError(new ApiForbiddenNoRightsError($"update {typeof(TModel).Name}")));
                }

                using (benchmarkSection.CreateBenchmark("UpdateDbModel"))
                {
                    for (var i = 0; i < currentModels.Length; i++)
                    {
                        var model = models[i];
                        var updatedPropertiesSingle = updatedProperties[i];
                        var currentDbModel = currentDbModels[i];

                        var updatedDbModel = await UpdateDbModel(db, session, currentDbModel, model, updatedPropertiesSingle);

                        if (updatedDbModel is IOptimisticLockable lockableDbModel)
                            db.Entry(updatedDbModel).OriginalValues[nameof(IOptimisticLockable.SysStartTime)] = lockableDbModel.SysStartTime;
                    }
                }

                using (benchmarkSection.CreateBenchmark("SaveChangesAsync"))
                {
                    try
                    {
                        await db.SaveChangesAsync(state);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw new ApiErrorException(
                            new ApiOptimisticLockingError<TModel>($"The model {typeof(TModel).Name} has changed."));
                    }
                    catch (DbUpdateException e)
                    {
                        throw TranslateDbUpdateExceptionCore(e);
                    }
                    catch (DbEntityValidationException e)
                    {
                        throw TranslateDbEntityValidationException(e);
                    }
                }

                using (benchmarkSection.CreateBenchmark("InternalGetByIdsAsync"))
                {
                    models = await InternalGetByIdsAsync(db, session, ids, false, false, benchmarkSection);

                    if (models.Any(m => m == null))
                        throw new Exception($"Could not find element by id after batch update (Entity = {typeof(TModel).Name}).");
                }

                return models;
            }
        }

        #endregion
        #region Delete

        /// <inheritdoc />
        public async Task InternalDeleteAsync(
            TDbContext db,
            TSession session,
            TState state,
            TId id,
            BenchmarkSection benchmarks)
        {
            using (var benchmarkSection = benchmarks.CreateSection($"{nameof(InternalDeleteAsync)}<{typeof(TModel).Name}>"))
            {
                TDbModel currentDbModel;
                TModel currentModel;
                using (benchmarkSection.CreateBenchmark("Query Current Models"))
                {
                    var dbModelQuery = FilterDbModelCollection(GetDbSet(db))
                        .Where(d => d.Id.Equals(id)); // TODO: check
                    var dbMetaDataQuery = SelectDbMetaData(db, session, dbModelQuery);

                    var currentModelData = (await dbModelQuery
                                               .Join(dbMetaDataQuery,
                                                   dbModel => dbModel.Id,
                                                   dbModelMetaData => dbModelMetaData.Id,
                                                   (dbModel, dbModelMetaData) => new
                                                   {
                                                       DbModel = dbModel,
                                                       DbModelMetaData = dbModelMetaData
                                                   })
                                               .SingleOrDefaultAsync())
                                           ?? throw new ApiErrorException(TranslateApiError(ApiNotFoundError.Create(typeof(TModel).Name, id)));

                    currentDbModel = currentModelData.DbModel;
                    currentModel = DbModelToModel(currentModelData.DbModelMetaData);
                    currentModel.AttachService(Service, session);
                }

                await DeleteAsyncCore(
                    db, 
                    session, 
                    new[] { currentDbModel }, 
                    new[] { currentModel },
                    benchmarkSection);

                using (benchmarkSection.CreateBenchmark("SaveChangesAsync"))
                {
                    try
                    {
                        await db.SaveChangesAsync(state);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw new ApiErrorException(
                            new ApiOptimisticLockingError<TModel>($"The model {typeof(TModel).Name} has changed."));
                    }
                    catch (DbUpdateException e)
                    {
                        throw TranslateDbUpdateExceptionCore(e);
                    }
                    catch (DbEntityValidationException e)
                    {
                        throw TranslateDbEntityValidationException(e);
                    }
                }
            }
        }
        /// <inheritdoc />
        public async Task InternalBatchDeleteAsync(
            TDbContext db,
            TSession session,
            TState state,
            TId[] ids,
            BenchmarkSection benchmarks)
        {
            if (ids.Length == 0)
                return;

            using (var benchmarkSection = benchmarks.CreateSection($"{nameof(InternalDeleteAsync)}<{typeof(TModel).Name}>"))
            {
                var currentDbModels = new TDbModel[ids.Length];
                var currentModels = new TModel[ids.Length];
                using (benchmarkSection.CreateBenchmark("Query Current Models"))
                {
                    var dbModelQuery = FilterDbModelCollection(GetDbSet(db))
                        .Where(d => ids.Contains(d.Id));
                    var dbMetaDataQuery = SelectDbMetaData(db, session, dbModelQuery);

                    var currentModelDataById = await dbModelQuery
                                               .Join(dbMetaDataQuery,
                                                   dbModel => dbModel.Id,
                                                   dbModelMetaData => dbModelMetaData.Id,
                                                   (dbModel, dbModelMetaData) => new
                                                   {
                                                       DbModel = dbModel,
                                                       DbModelMetaData = dbModelMetaData
                                                   })
                                               .ToDictionaryAsync(x => x.DbModel.Id);

                    if (ids.Length != currentModelDataById.Count)
                    {
                        throw new ApiErrorException(ids
                            .Where(id => !currentModelDataById.ContainsKey(id))
                            .Select(id => TranslateApiError(ApiNotFoundError.Create(typeof(TModel).Name, id)))
                            .ToArray());
                    }

                    var index = 0;
                    foreach (var currentModelData in currentModelDataById.Values)
                    {
                        currentDbModels[index] = currentModelData.DbModel;
                        var model = DbModelToModel(currentModelData.DbModelMetaData);
                        model.AttachService(Service, session);
                        currentModels[index] = model;
                        ++ index;
                    }
                }

                await DeleteAsyncCore(
                    db, 
                    session, 
                    currentDbModels, 
                    currentModels,
                    benchmarkSection);

                using (benchmarkSection.CreateBenchmark("SaveChangesAsync"))
                {
                    try
                    {
                        await db.SaveChangesAsync(state);
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw new ApiErrorException(new ApiOptimisticLockingError<TModel>($"The model {typeof(TModel).Name} has changed."));
                    }
                    catch (DbUpdateException e)
                    {
                        throw TranslateDbUpdateExceptionCore(e);
                    }
                    catch (DbEntityValidationException e)
                    {
                        throw TranslateDbEntityValidationException(e);
                    }
                }
            }
        }

        [NotNull]
        private async Task DeleteAsyncCore(
            [NotNull] TDbContext db,
            [NotNull] TSession session,
            [NotNull, ItemNotNull] IReadOnlyCollection<TDbModel> dbModels,
            [NotNull, ItemNotNull] IEnumerable<TModel> models,
            BenchmarkSection benchmarks)
        {
            if (dbModels.Count == 0)
                return;

            using (var benchmarkSection = benchmarks.CreateSection(nameof(DeleteAsyncCore)))
            {
                using (benchmarkSection.CreateBenchmark("Permission check"))
                {
                    var deniedErrors = models
                        .Where(model => !PermissionResolverHelper.CanAccessRecord(session.PermissionResolver, AccessType.Delete, model))
                        .Select(model => TranslateApiError(new ApiForbiddenNoRightsError($"delete {typeof(TModel).Name}")))
                        .ToArray();

                    if (deniedErrors.Any())
                        throw new ApiErrorException(deniedErrors);
                }

                using (benchmarkSection.CreateBenchmark("DeleteDbModel"))
                {
                    foreach (var dbModel in dbModels)
                    {
                        if (dbModel is IOptimisticLockable lockableDbModel)
                            db.Entry(dbModel).OriginalValues[nameof(IOptimisticLockable.SysStartTime)] = lockableDbModel.SysStartTime;

                        await DeleteDbModel(db, session, dbModel);
                    }
                }
            }
        }
        #endregion

        #endregion


        #region Helpers to set fields

        /// <summary>
        /// Updates a to-many relation.
        /// 
        /// Adds/removes ids to the relation to match the passed <c>value</c>.
        /// 
        /// If no add- or remove-operations are needed, nothing happens.
        /// 
        /// If any operation is performed, the primary record is also flagged as modified and therefore is optimistically locked.
        /// </summary>
        /// <typeparam name="TRelation">The type of the relation record.</typeparam>
        /// <param name="model">The primary record.</param>
        /// <param name="value">The new values of the relation.</param>
        /// <param name="db">The db on which to work.</param>
        /// <param name="getRelation">A selector that returns the relations for a given model.</param>
        /// <param name="setRelation">An action that sets the relation-collection for a given model. Is only called if <c>getRelation</c> returns null.</param>
        /// <param name="getRelatedId">A selector that returns the id from a relation.</param>
        /// <param name="newRelationForId">An action that creates a new relation for an id.</param>
        protected void UpdateDbModelToManyRelation<TRelation>(
                [NotNull] TDbModel model,
                [NotNull] IEnumerable<long> value,
                [NotNull] TDbContext db,
                [NotNull] Func<TDbModel, ICollection<TRelation>> getRelation,
                [NotNull] Action<TDbModel, ICollection<TRelation>> setRelation,
                [NotNull] Func<TRelation, long> getRelatedId,
                [NotNull] Func<long, TRelation> newRelationForId)
            where TRelation : class, IId<long>
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (db == null)
                throw new ArgumentNullException(nameof(db));
            if (getRelation == null)
                throw new ArgumentNullException(nameof(getRelation));
            if (setRelation == null)
                throw new ArgumentNullException(nameof(setRelation));
            if (getRelatedId == null)
                throw new ArgumentNullException(nameof(getRelatedId));
            if (newRelationForId == null)
                throw new ArgumentNullException(nameof(newRelationForId));

            var relation = getRelation(model);
            if (relation == null)
            {
                relation = new List<TRelation>();
                setRelation(model, relation);
            }

            var current = relation.ToDictionary(getRelatedId);
            var toBe = value.ToHashSet();

            var add = toBe.Where(x => !current.ContainsKey(x)).ToArray();
            var delete = current.Where(x => !toBe.Contains(x.Key)).Select(x => x.Value).ToArray();

            if (!(add.Any() || delete.Any()))
                return;

            // If the relation changes, also set the model to Modified.
            // This updates the optimistic concurrency field and supports locking.
            var entry = db.Entry(model);
            if (entry.State == EntityState.Unchanged)
                entry.State = EntityState.Modified;

            // Remove relation-entities from model
            foreach (var item in delete)
            {
                db.Entry(item).State = EntityState.Deleted;
                relation.Remove(item);
            }

            // Add new relation-entities
            foreach (var id in add)
            {
                var item = newRelationForId(id);
                relation.Add(item);
                db.Entry(item).State = EntityState.Added;
            }
        }
        #endregion
    }
}