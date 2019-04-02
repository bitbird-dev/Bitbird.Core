using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public interface IDataContextEntityHookEvents<TDataContext, TState>
        where TDataContext : System.Data.Entity.DbContext
    {
        Task InvokePreInsertAsync(TDataContext db, TState state, EntityHookEvent[] entities);
        Task InvokePostInsertAsync(TDataContext db, TState state, EntityHookEvent[] entities);
        Task InvokePreDeleteAsync(TDataContext db, TState state, EntityHookEvent[] entities);
        Task InvokePostDeleteAsync(TDataContext db, TState state, EntityHookEvent[] entities);
        Task InvokePreUpdateAsync(TDataContext db, TState state, EntityHookEvent[] entitiesHookEvent);
        Task InvokePostUpdateAsync(TDataContext db, TState state, EntityHookEvent[] entitiesHookEvent);

        IDataContextEntityHookEvents<TDataContext, TState> Clone();
    }
}