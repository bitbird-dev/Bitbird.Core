using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Calls.Core
{
    // ReSharper disable UnusedTypeParameter
    public abstract partial class ServiceCrudNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
    // ReSharper restore UnusedTypeParameter
    {
        [NotNull]
        protected abstract ICallBase<TModel> CreateUpdateCall([NotNull] TSession session, [NotNull] TModel model, [CanBeNull] Func<string, bool> updatedProperty);
        
        /// <inheritdoc />
        public virtual Task<TModel> UpdateAsync(TSession session, TModel model, Func<string, bool> updatedProperty)
        {
            return CreateUpdateCall(session, model, updatedProperty).ExecuteAsync();
        }
    }
}