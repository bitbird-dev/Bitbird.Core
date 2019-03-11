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

        public async Task InvokePreInsertAsync(TDataContext db, EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in preEventAsyncHandlers)
                await a(db, data, HookEventType.Insert);
        }
        public async Task InvokePostInsertAsync(TDataContext db, EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in postEventAsyncHandlers)
                await a(db, data, HookEventType.Insert);
        }
        public async Task InvokePreDeleteAsync(TDataContext db, EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in preEventAsyncHandlers)
                await a(db, data, HookEventType.Delete);
        }
        public async Task InvokePostDeleteAsync(TDataContext db, EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in postEventAsyncHandlers)
                await a(db, data, HookEventType.Delete);
        }
        public async Task InvokePreUpdateAsync(TDataContext db, EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in preEventAsyncHandlers)
                await a(db, data, HookEventType.Update);
        }
        public async Task InvokePostUpdateAsync(TDataContext db, EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in postEventAsyncHandlers)
                await a(db, data, HookEventType.Update);
        }
    }
}