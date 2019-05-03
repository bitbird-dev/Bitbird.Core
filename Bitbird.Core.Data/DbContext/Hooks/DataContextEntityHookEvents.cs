using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.DbContext.Hooks
{
    public class DataContextEntityHookEvents<TDataContext, T, TState> : IDataContextEntityHookEvents<TDataContext, TState>
        where T : class
    {
        private readonly List<Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task>> postEventAsyncHandlers;
        private readonly List<Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task>> preEventAsyncHandlers;

        public DataContextEntityHookEvents() 
            : this(new List<Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task>>(), 
                   new List<Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task>>())
        {
        }
        private DataContextEntityHookEvents(
            List<Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task>> postEventAsyncHandlers, 
            List<Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task>> preEventAsyncHandlers)
        {
            this.postEventAsyncHandlers = postEventAsyncHandlers;
            this.preEventAsyncHandlers = preEventAsyncHandlers;
        }
        
        IDataContextEntityHookEvents<TDataContext, TState> IDataContextEntityHookEvents<TDataContext, TState>.Clone() 
            => Clone();
        public DataContextEntityHookEvents<TDataContext, T, TState> Clone() 
            => new DataContextEntityHookEvents<TDataContext, T, TState>(postEventAsyncHandlers.ToList(), preEventAsyncHandlers.ToList());

        public void AddPostEventAsyncHandler(Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task> handler)
            => postEventAsyncHandlers.Add(handler);
        public void AddPreEventAsyncHandler(Func<TDataContext, TState, EntityHookEvent<T>[], HookEventType, Task> handler)
            => preEventAsyncHandlers.Add(handler);

        public Task InvokePreInsertAsync(TDataContext db, TState state, EntityHookEvent[] entities) => InvokePreAsync(db, state, entities, HookEventType.Insert);
        public Task InvokePostInsertAsync(TDataContext db, TState state, EntityHookEvent[] entities) => InvokePostAsync(db, state, entities, HookEventType.Insert);
        public Task InvokePreDeleteAsync(TDataContext db, TState state, EntityHookEvent[] entities) => InvokePreAsync(db, state, entities, HookEventType.Delete);
        public Task InvokePostDeleteAsync(TDataContext db, TState state, EntityHookEvent[] entities) => InvokePostAsync(db, state, entities, HookEventType.Delete);
        public Task InvokePreUpdateAsync(TDataContext db, TState state, EntityHookEvent[] entities) => InvokePreAsync(db, state, entities, HookEventType.Update);
        public Task InvokePostUpdateAsync(TDataContext db, TState state, EntityHookEvent[] entities) => InvokePostAsync(db, state, entities, HookEventType.Update);


        private async Task InvokePreAsync(TDataContext db, TState state, EntityHookEvent[] entities, HookEventType type)
        {
            var data = entities
                .Select(entity => entity.ToTyped<T>())
                .ToArray();

            foreach (var a in preEventAsyncHandlers)
                await a(db, state, data, type); // Don't use Task.WhenAll. Don't run in parallel. The Entity Framework data context does not allow parallel queries.
        }
        private async Task InvokePostAsync(TDataContext db, TState state, EntityHookEvent[] entities, HookEventType type)
        {
            var data = entities
                .Select(entity => entity.ToTyped<T>())
                .ToArray();

            foreach (var a in postEventAsyncHandlers)
                await a(db, state, data, type); // Don't use Task.WhenAll. Don't run in parallel. The Entity Framework data context does not allow parallel queries.
        }
    }
}