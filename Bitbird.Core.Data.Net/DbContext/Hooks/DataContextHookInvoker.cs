using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookInvoker
    {
        private readonly System.Data.Entity.DbContext context;
        private readonly DataContextHookCollection hooks;
        private readonly Dictionary<EntityState, List<DbEntityEntry>> entries = new Dictionary<EntityState, List<DbEntityEntry>>
        {
            { EntityState.Added, new List<DbEntityEntry>() },
            { EntityState.Deleted, new List<DbEntityEntry>() },
            { EntityState.Modified, new List<DbEntityEntry>() }
        };

        public DataContextHookInvoker(System.Data.Entity.DbContext context, DataContextHookCollection hooks)
        {
            this.context = context;
            this.hooks = hooks;

            if (!(hooks?.HasHooks ?? false))
                return;

            foreach (var entry in context.ChangeTracker.Entries())
                if (entries.TryGetValue(entry.State, out var list))
                    list.Add(entry);
        }

        public void InvokePreHooks()
        {
            if (!(hooks?.HasHooks ?? false) || (context.Configuration.ValidateOnSaveEnabled && entries.Values.Any(list => list.Any(x => !x.GetValidationResult().IsValid))))
                return;

            foreach (var entry in entries[EntityState.Added])
                hooks.InvokePreInsert(entry.Entity);
            foreach (var entry in entries[EntityState.Modified])
                hooks.InvokePreUpdate(entry.Entity);
            foreach (var entry in entries[EntityState.Deleted])
                hooks.InvokePreDelete(entry.Entity);
        }

        public void InvokePostHooks()
        {
            if (!(hooks?.HasHooks ?? false))
                return;

            foreach (var entry in entries[EntityState.Added])
                hooks.InvokePostInsert(entry.Entity);
            foreach (var entry in entries[EntityState.Modified])
                hooks.InvokePostUpdate(entry.Entity);
            foreach (var entry in entries[EntityState.Deleted])
                hooks.InvokePostDelete(entry.Entity);
        }
    }
}