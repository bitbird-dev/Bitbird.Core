using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.DbContext
{
    public interface IHookedStateDataContext<TState>
    {
        [NotNull]
        Task<int> SaveChangesAsync(TState state);

        [NotNull]
        Task<int> SaveChangesAsync(TState state, CancellationToken cancellationToken);
    }
}