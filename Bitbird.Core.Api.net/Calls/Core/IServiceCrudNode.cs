using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Calls.Core
{
    public interface IServiceCrudNode<TSession, TModel, TId>
        : IServiceReadNode<TSession, TModel, TId>
        where TModel : class, IId<TId>
        where TSession : class, IApiSession
    {
        [NotNull, ItemNotNull]
        Task<TModel> CreateAsync(
            [NotNull] TSession apiSession, 
            [NotNull] TModel model);

        [NotNull, ItemNotNull]
        Task<TModel[]> CreateManyAsync(
            [NotNull] TSession apiSession, 
            [NotNull, ItemNotNull] TModel[] models);

        [NotNull]
        Task DeleteAsync(
            [NotNull] TSession apiSession,
            TId id);

        [NotNull, ItemNotNull]
        Task<TModel> UpdateAsync(
            [NotNull] TSession apiSession,
            [NotNull] TModel model, 
            [CanBeNull] Func<string, bool> updatedProperty);
    }
}