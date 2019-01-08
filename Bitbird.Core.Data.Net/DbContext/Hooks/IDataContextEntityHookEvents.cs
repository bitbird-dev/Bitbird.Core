using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public interface IDataContextEntityHookEvents
    {
        Task InvokePreInsertAsync(EntityHookEvent[] entities);
        Task InvokePostInsertAsync(EntityHookEvent[] entities);
        Task InvokePreDeleteAsync(EntityHookEvent[] entities);
        Task InvokePostDeleteAsync(EntityHookEvent[] entities);
        Task InvokePreUpdateAsync(EntityHookEvent[] entitiesHookEvent);
        Task InvokePostUpdateAsync(EntityHookEvent[] entitiesHookEvent);

        IDataContextEntityHookEvents Clone();
    }
}