using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public interface IDataContextEntityHookEvents<TDataContext>
        where TDataContext : System.Data.Entity.DbContext
    {
        Task InvokePreInsertAsync(TDataContext db, EntityHookEvent[] entities);
        Task InvokePostInsertAsync(TDataContext db, EntityHookEvent[] entities);
        Task InvokePreDeleteAsync(TDataContext db, EntityHookEvent[] entities);
        Task InvokePostDeleteAsync(TDataContext db, EntityHookEvent[] entities);
        Task InvokePreUpdateAsync(TDataContext db, EntityHookEvent[] entitiesHookEvent);
        Task InvokePostUpdateAsync(TDataContext db, EntityHookEvent[] entitiesHookEvent);

        IDataContextEntityHookEvents<TDataContext> Clone();
    }
}