using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Calls.Core
{
    // ReSharper disable UnusedTypeParameter
    public abstract partial class ServiceCrudNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
    // ReSharper restore UnusedTypeParameter
    {
        [NotNull]
        protected abstract ICallBase<TModel> CreateCreateCall([NotNull] TSession session, [NotNull] TModel model);

        /// <inheritdoc />
        public virtual Task<TModel> CreateAsync(TSession session, TModel model)
        {
            return CreateCreateCall(session, model).ExecuteAsync();
        }
    }
}