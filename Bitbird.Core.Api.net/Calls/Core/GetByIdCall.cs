using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Calls.Core
{
    // ReSharper disable UnusedTypeParameter
    public abstract partial class ServiceReadNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
    // ReSharper restore UnusedTypeParameter
    {
        [NotNull]
        protected abstract ICallBase<TModel> CreateGetByIdCall([NotNull] TSession session, [CanBeNull] TId id);

        /// <inheritdoc />
        public virtual Task<TModel> GetByIdAsync(TSession session, TId id)
        {
            return CreateGetByIdCall(session, id).ExecuteAsync();
        }
    }
}