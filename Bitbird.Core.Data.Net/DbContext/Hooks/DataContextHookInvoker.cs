using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookInvoker<TDataContext, TState>
        where TDataContext : System.Data.Entity.DbContext
    {
        private readonly TDataContext context;
        private readonly DataContextHookCollection<TDataContext, TState> hooks;
        private readonly TState state;
        private readonly EntityHookEvent[] addedEntries;
        private readonly EntityHookEvent[] modifiedEntries;
        private readonly EntityHookEvent[] deletedEntries;

        public DataContextHookInvoker(TDataContext context, DataContextHookCollection<TDataContext, TState> hooks, TState state)
        {
            this.context = context;
            this.hooks = hooks;
            this.state = state;

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
                        if (entry.Entity is IIsDeletedFlagEntity)
                        {
                            var newIsDeleted = (bool)entry.CurrentValues[nameof(IIsDeletedFlagEntity.IsDeleted)];
                            if (newIsDeleted)
                                break;
                        }

                        addedEntriesList.Add(new EntityHookEvent(null, entry.Entity));
                        break;

                    case EntityState.Modified:
                        if (entry.Entity is IIsDeletedFlagEntity)
                        {
                            var oldIsDeleted = (bool)entry.OriginalValues[nameof(IIsDeletedFlagEntity.IsDeleted)];
                            var newIsDeleted = (bool)entry.CurrentValues[nameof(IIsDeletedFlagEntity.IsDeleted)];

                            if (oldIsDeleted != newIsDeleted)
                            {
                                if (newIsDeleted)
                                    deletedEntriesList.Add(new EntityHookEvent(entry.Entity, null));
                                else
                                    addedEntriesList.Add(new EntityHookEvent(null, entry.Entity));

                                break;
                            }
                        }

                        var original = entry.OriginalValues.ToObject();
                        modifiedEntriesList.Add(new EntityHookEvent(context.Entry(original).Entity, entry.Entity));
                        break;

                    case EntityState.Deleted:
                        if (entry.Entity is IIsDeletedFlagEntity)
                        {
                            var oldIsDeleted = (bool)entry.OriginalValues[nameof(IIsDeletedFlagEntity.IsDeleted)];
                            if (oldIsDeleted)
                                break;
                        }

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
                await hooks.InvokePreInsertAsync(context, state, addedEntries);

            if (modifiedEntries.Length != 0)
                await hooks.InvokePreUpdateAsync(context, state, modifiedEntries);

            if (deletedEntries.Length != 0)
                await hooks.InvokePreDeleteAsync(context, state, deletedEntries);
        }

        public async Task InvokePostHooksAsync()
        {
            if (hooks == null)
                return;

            if (addedEntries.Length != 0)
                await hooks.InvokePostInsertAsync(context, state, addedEntries);

            if (modifiedEntries.Length != 0)
                await hooks.InvokePostUpdateAsync(context, state, modifiedEntries);

            if (deletedEntries.Length != 0)
                await hooks.InvokePostDeleteAsync(context, state, deletedEntries);
        }
    }
}