using System;
using System.Threading.Tasks;
using Bitbird.Core.Api.Models.Base;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data.DbContext;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Calls.Core
{
    public interface IInternalCrudNode<TService, TSession, TDbContext, TState, TModel, TEntityChangeModel, TEntityTypeId, TId>
        : IInternalReadNode<TService, TSession, TDbContext, TState, TModel, TEntityChangeModel, TEntityTypeId, TId>
        where TModel : ModelBase<TService, TSession, TDbContext, TState, TEntityChangeModel, TEntityTypeId, TId>, IId<TId>
        where TService : class, IApiService<TDbContext, TState, TSession, TEntityChangeModel, TEntityTypeId, TId>
        where TDbContext : class, IDisposable, IHookedStateDataContext<TState>
        where TState : IBaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId>
        where TSession : class, IApiSession
        where TEntityChangeModel : class
    {
        /// <summary>
        /// Persists the passed model and returns the created model (which might differ from the passed model in some calculated/related attributes, e.g. Id).
        /// Permissions are checked before creation (needed permission: AccessType.Create for the model type AND AccessType.Read for the model and related entities which are needed for model creation if queryCreated is true), and an ApiErrorException(ApiForbiddenNoRightsError) is raised if the check fails.
        /// </summary>
        /// <param name="db">The data context to use. Must not be null. <see cref="Data.Net.DbContext.HookedStateDataContext{TDataContext, TState}.SaveChangesAsync(TState, System.Threading.CancellationToken)"/> is called on the context.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="state">A state object storing that is passed to the database hooks invoker.</param>
        /// <param name="model">The model which should be created. Must not be null. Some attributes might be ignored during the creation process. For more information see the documentation of the model.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The created model. See documentation of the queryCreated-parameter for details.</returns>
        [NotNull, ItemNotNull]
        Task<TModel> InternalCreateAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TState state,
            [NotNull] TModel model,
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Persists the passed models and returns the created models (which might differ from the passed models in some calculated/related attributes, e.g. Id).
        /// Permissions are checked before creation (needed permission: AccessType.Create for the model type AND AccessType.Read for the model and related entities which are needed for model creation if queryCreated is true), and an ApiErrorException(ApiForbiddenNoRightsError) is raised if the check fails.
        /// Returns models in the same order as their corresponding models in the models-parameter.
        /// </summary>
        /// <param name="db">The data context to use. Must not be null. <see cref="Data.Net.DbContext.HookedStateDataContext{TDataContext, TState}.SaveChangesAsync(TState, System.Threading.CancellationToken)"/> is called on the context.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="state">A state object storing that is passed to the database hooks invoker.</param>
        /// <param name="models">The models which should be created. Must not be null. Some attributes might be ignored during the creation process. For more information see the documentation of the model.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The created models. See documentation of the queryCreated-parameter for details.</returns>
        [NotNull, ItemNotNull]
        Task<TModel[]> InternalBatchCreateAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TState state,
            [NotNull, ItemNotNull] TModel[] models,
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Persists changes in the passed model and returns the updated model (which might differ from the passed model in some calculated/related attributes, e.g. Id).
        /// Permissions are checked before update (needed permission: AccessType.Update for the model type AND AccessType.Read for the model type and related entities which are needed for model creation), and an ApiErrorException(ApiForbiddenNoRightsError) is raised if the check fails.
        /// </summary>
        /// <param name="db">The data context to use. Must not be null. <see cref="Data.Net.DbContext.HookedStateDataContext{TDataContext, TState}.SaveChangesAsync(TState, System.Threading.CancellationToken)"/> is called on the context.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="state">A state object storing that is passed to the database hooks invoker.</param>
        /// <param name="model">The model which should be updated. Must not be null. Some attributes might be ignored during the update process. For more information see the documentation of the model.</param>
        /// <param name="updatedProperties">A predicate that is passed Property-names as string. If the predicate is null or returns true the property will be updated (If the model-class supports it).</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The updated model. See documentation of the queryCreated-parameter for details.</returns>
        [NotNull, ItemNotNull]
        Task<TModel> InternalUpdateAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TState state,
            [NotNull] TModel model,
            [CanBeNull] Func<string, bool> updatedProperties,
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Persists changes in the passed models and returns the updated models (which might differ from the passed models in some calculated/related attributes, e.g. Id).
        /// Permissions are checked before update (needed permission: AccessType.Update for the model type AND AccessType.Read for the model type and related entities which are needed for model creation), and an ApiErrorException(ApiForbiddenNoRightsError) is raised if the check fails.
        /// Returns models in the same order as their corresponding models in the models-parameter.
        /// </summary>
        /// <param name="db">The data context to use. Must not be null. <see cref="Data.Net.DbContext.HookedStateDataContext{TDataContext, TState}.SaveChangesAsync(TState, System.Threading.CancellationToken)"/> is called on the context.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="state">A state object storing that is passed to the database hooks invoker.</param>
        /// <param name="models">The models which should be updated. Must not be null. Some attributes might be ignored during the update process. For more information see the documentation of the model.</param>
        /// <param name="updatedProperties">Predicates that are passed Property-names as string. If the predicate is null or returns true the property will be updated (If the model-class supports it). The passed array must not be null.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The updated models. See documentation of the queryCreated-parameter for details.</returns>
        [NotNull, ItemNotNull]
        Task<TModel[]> InternalBatchUpdateAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TState state,
            [NotNull] [ItemNotNull] TModel[] models,
            [NotNull] Func<string, bool>[] updatedProperties, 
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Deletes the model with the passed id.
        /// Permissions are checked before delete (needed permission: AccessType.Delete for the model type AND AccessType.Read for the model type and related entities which are needed for model creation), and an ApiErrorException(ApiForbiddenNoRightsError) is raised if the check fails.
        /// </summary>
        /// <param name="db">The data context to use. Must not be null. <see cref="Data.Net.DbContext.HookedStateDataContext{TDataContext, TState}.SaveChangesAsync(TState, System.Threading.CancellationToken)"/> is called on the context.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="state">A state object storing that is passed to the database hooks invoker.</param>
        /// <param name="id">The id of the model to delete.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>Nothing.</returns>
        [NotNull]
        Task InternalDeleteAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TState state,
            TId id,
            [CanBeNull] BenchmarkSection benchmarks = null);

        /// <summary>
        /// Deletes the models with the passed ids.
        /// Permissions are checked before delete (needed permission: AccessType.Delete for the model type AND AccessType.Read for the model type and related entities which are needed for model creation), and an ApiErrorException(ApiForbiddenNoRightsError) is raised if the check fails.
        /// </summary>
        /// <param name="db">The data context to use. Must not be null. <see cref="Data.Net.DbContext.HookedStateDataContext{TDataContext, TState}.SaveChangesAsync(TState, System.Threading.CancellationToken)"/> is called on the context.</param>
        /// <param name="apiSession">The current user session.</param>
        /// <param name="state">A state object storing that is passed to the database hooks invoker.</param>
        /// <param name="ids">The isd of the models to delete.</param>
        /// <param name="benchmarks">A benchmark section to use. Can be null.</param>
        /// <returns>The updated models. See documentation of the queryCreated-parameter for details.</returns>
        [NotNull]
        Task InternalBatchDeleteAsync(
            [NotNull] TDbContext db,
            [NotNull] TSession apiSession,
            [NotNull] TState state,
            [NotNull] TId[] ids,
            [CanBeNull] BenchmarkSection benchmarks = null);
    }
}