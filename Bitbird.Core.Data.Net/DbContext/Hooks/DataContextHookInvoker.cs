using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookInvoker<TDataContext>
        where TDataContext : System.Data.Entity.DbContext
    {
        private readonly TDataContext context;
        private readonly DataContextHookCollection<TDataContext> hooks;
        private readonly EntityHookEvent[] addedEntries;
        private readonly EntityHookEvent[] modifiedEntries;
        private readonly EntityHookEvent[] deletedEntries;

        public DataContextHookInvoker(TDataContext context, DataContextHookCollection<TDataContext> hooks)
        {
            this.context = context;
            this.hooks = hooks;

            if (hooks == null)
            {
                addedEntries = null;
                modifiedEntries = null;
                deletedEntries = null;
                return;
            }

            var addedEntriesList = new List<EntityHookEvent>();
            var modifiedEntriesList = new List<EntityHookEvent>();
            var deletedEntriesList = new List<EntityHookEvent>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        addedEntriesList.Add(new EntityHookEvent(null, entry.Entity));
                        break;
                    case EntityState.Modified:
                        var original = entry.OriginalValues.ToObject();
                        modifiedEntriesList.Add(new EntityHookEvent(context.Entry(original).Entity, entry.Entity));
                        break;
                    case EntityState.Deleted:
                        deletedEntriesList.Add(new EntityHookEvent(entry.Entity, null));
                        break;
                }
            }

            addedEntries = addedEntriesList.ToArray();
            modifiedEntries = modifiedEntriesList.ToArray();
            deletedEntries = deletedEntriesList.ToArray();
        }

        public async Task InvokePreHooksAsync()
        {
            if (hooks == null || (context.Configuration.ValidateOnSaveEnabled && context.ChangeTracker.Entries().Any(x => !x.GetValidationResult().IsValid)))
                return;

            if (addedEntries.Length != 0)
                await hooks.InvokePreInsertAsync(context, addedEntries);

            if (modifiedEntries.Length != 0)
                await hooks.InvokePreUpdateAsync(context, modifiedEntries);

            if (deletedEntries.Length != 0)
                await hooks.InvokePreDeleteAsync(context, deletedEntries);
        }

        public async Task InvokePostHooksAsync()
        {
            if (hooks == null)
                return;

            if (addedEntries.Length != 0)
                await hooks.InvokePostInsertAsync(context, addedEntries);

            if (modifiedEntries.Length != 0)
                await hooks.InvokePostUpdateAsync(context, modifiedEntries);

            if (deletedEntries.Length != 0)
                await hooks.InvokePostDeleteAsync(context, deletedEntries);
        }
    }
}