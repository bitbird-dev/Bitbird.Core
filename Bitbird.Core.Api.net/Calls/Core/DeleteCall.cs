using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Calls.Core
{
    // ReSharper disable UnusedTypeParameter
    public abstract partial class ServiceCrudNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
    // ReSharper restore UnusedTypeParameter
    {
        [NotNull]
        protected abstract ICallBase<object> CreateDeleteCall([NotNull] TSession session, [CanBeNull] TId id);

        /// <inheritdoc />
        public virtual Task DeleteAsync(TSession session, TId id)
        {
            return CreateDeleteCall(session, id).ExecuteAsync();
        }
    }
}