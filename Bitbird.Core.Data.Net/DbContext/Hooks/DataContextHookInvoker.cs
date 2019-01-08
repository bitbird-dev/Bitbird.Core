using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookInvoker
    {
        private readonly System.Data.Entity.DbContext context;
        private readonly DataContextHookCollection hooks;
        private readonly EntityHookEvent[] addedEntries;
        private readonly EntityHookEvent[] modifiedEntries;
        private readonly EntityHookEvent[] deletedEntries;

        public DataContextHookInvoker(System.Data.Entity.DbContext context, DataContextHookCollection hooks)
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
                await hooks.InvokePreInsertAsync(addedEntries);

            if (modifiedEntries.Length != 0)
                await hooks.InvokePreUpdateAsync(modifiedEntries);

            if (deletedEntries.Length != 0)
                await hooks.InvokePreDeleteAsync(deletedEntries);
        }

        public async Task InvokePostHooksAsync()
        {
            if (hooks == null)
                return;

            if (addedEntries.Length != 0)
                await hooks.InvokePostInsertAsync(addedEntries);

            if (modifiedEntries.Length != 0)
                await hooks.InvokePostUpdateAsync(modifiedEntries);

            if (deletedEntries.Length != 0)
                await hooks.InvokePostDeleteAsync(deletedEntries);
        }
    }
}