using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextEntityHookEvents<TDataContext, T> : IDataContextEntityHookEvents<TDataContext>
        where TDataContext : System.Data.Entity.DbContext
        where T : class
    {
        private readonly List<Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task>> postEventAsyncHandlers;
        private readonly List<Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task>> preEventAsyncHandlers;

        public DataContextEntityHookEvents() 
            : this(new List<Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task>>(), 
                   new List<Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task>>())
        {
        }
        private DataContextEntityHookEvents(List<Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task>> postEventAsyncHandlers, List<Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task>> preEventAsyncHandlers)
        {
            this.postEventAsyncHandlers = postEventAsyncHandlers;
            this.preEventAsyncHandlers = preEventAsyncHandlers;
        }
        
        IDataContextEntityHookEvents<TDataContext> IDataContextEntityHookEvents<TDataContext>.Clone() 
            => Clone();
        public DataContextEntityHookEvents<TDataContext, T> Clone() 
            => new DataContextEntityHookEvents<TDataContext, T > (postEventAsyncHandlers.ToList(), preEventAsyncHandlers.ToList());

        public void AddPostEventAsyncHandler(Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task> handler)
            => postEventAsyncHandlers.Add(handler);
        public void AddPreEventAsyncHandler(Func<TDataContext, EntityHookEvent<T>[], HookEventType, Task> handler)
            => preEventAsyncHandlers.Add(handler);

        public Task InvokePreInsertAsync(TDataContext db, EntityHookEvent[] entities) => InvokePreAsync(db, entities, HookEventType.Insert);
        public Task InvokePostInsertAsync(TDataContext db, EntityHookEvent[] entities) => InvokePostAsync(db, entities, HookEventType.Insert);
        public Task InvokePreDeleteAsync(TDataContext db, EntityHookEvent[] entities) => InvokePreAsync(db, entities, HookEventType.Delete);
        public Task InvokePostDeleteAsync(TDataContext db, EntityHookEvent[] entities) => InvokePostAsync(db, entities, HookEventType.Delete);
        public Task InvokePreUpdateAsync(TDataContext db, EntityHookEvent[] entities) => InvokePreAsync(db, entities, HookEventType.Update);
        public Task InvokePostUpdateAsync(TDataContext db, EntityHookEvent[] entities) => InvokePostAsync(db, entities, HookEventType.Update);


        private async Task InvokePreAsync(TDataContext db, EntityHookEvent[] entities, HookEventType type)
        {
            var data = entities
                .Select(entity => entity.ToTyped<T>())
                .ToArray();

            foreach (var a in preEventAsyncHandlers)
                await a(db, data, type); // Don't use Task.WhenAll. Don't run in parallel. The Entity Framework data context does not allow parallel queries.
        }
        private async Task InvokePostAsync(TDataContext db, EntityHookEvent[] entities, HookEventType type)
        {
            var data = entities
                .Select(entity => entity.ToTyped<T>())
                .ToArray();

            foreach (var a in postEventAsyncHandlers)
                await a(db, data, type); // Don't use Task.WhenAll. Don't run in parallel. The Entity Framework data context does not allow parallel queries.
        }
    }
}