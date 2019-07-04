using System.Threading.Tasks;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Query;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Nodes.Core
{
    public interface IServiceReadNode<in TSession, TModel, in TId>
        where TModel : class, IId<TId>
        where TSession : class, IApiSession
    {
        [NotNull, ItemNotNull]
        Task<TModel> GetByIdAsync(
            [NotNull] TSession apiSession,
            TId id);

        [NotNull, ItemNotNull]
        Task<QueryResult<TModel>> GetManyAsync(
            [NotNull] TSession apiSession, 
            [CanBeNull] QueryInfo queryInfo = null);
    }
}