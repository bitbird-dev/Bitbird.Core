using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Calls.Core
{
    // ReSharper disable UnusedTypeParameter
    public abstract partial class ServiceCrudNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
    // ReSharper restore UnusedTypeParameter
    {
        [NotNull]
        protected abstract ICallBase<TModel[]> CreateCreateManyCall([NotNull] TSession session, [NotNull, ItemNotNull] TModel[] models);


        /// <inheritdoc />
        public virtual Task<TModel[]> CreateManyAsync(TSession session, TModel[] models)
        {
            return CreateCreateManyCall(session, models).ExecuteAsync();
        }
    }
}