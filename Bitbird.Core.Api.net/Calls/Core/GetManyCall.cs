using System.Threading.Tasks;
using Bitbird.Core.Data.Net.Query;
using Bitbird.Core.Query;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Calls.Core
{
    // ReSharper disable UnusedTypeParameter
    public abstract partial class ServiceReadNode<TService, TSession, TDbContext, TState, TModel, TDbModel, TDbMetaData, TRightId, TEntityTypeId, TEntityChangeModel, TId>
    // ReSharper restore UnusedTypeParameter
    {
        [NotNull]
        protected abstract ICallBase<QueryResult<TModel>> CreateGetManyCall([NotNull] TSession session, [CanBeNull] QueryInfo queryInfo);
        
        /// <inheritdoc />
        public virtual Task<QueryResult<TModel>> GetManyAsync(TSession session, QueryInfo queryInfo = null)
        {
            return CreateGetManyCall(session, queryInfo).ExecuteAsync();
        }
    }
}