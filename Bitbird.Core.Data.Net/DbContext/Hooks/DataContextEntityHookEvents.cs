using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextEntityHookEvents<T> : IDataContextEntityHookEvents
        where T : class
    {
        private readonly List<Func<EntityHookEvent<T>[], HookEventType, Task>> postEventAsyncHandlers;
        private readonly List<Func<EntityHookEvent<T>[], HookEventType, Task>> preEventAsyncHandlers;

        public DataContextEntityHookEvents() 
            : this(new List<Func<EntityHookEvent<T>[], HookEventType, Task>>(), 
                   new List<Func<EntityHookEvent<T>[], HookEventType, Task>>())
        {
        }
        private DataContextEntityHookEvents(List<Func<EntityHookEvent<T>[], HookEventType, Task>> postEventAsyncHandlers, List<Func<EntityHookEvent<T>[], HookEventType, Task>> preEventAsyncHandlers)
        {
            this.postEventAsyncHandlers = postEventAsyncHandlers;
            this.preEventAsyncHandlers = preEventAsyncHandlers;
        }
        
        IDataContextEntityHookEvents IDataContextEntityHookEvents.Clone() 
            => Clone();
        public DataContextEntityHookEvents<T> Clone() 
            => new DataContextEntityHookEvents<T>(postEventAsyncHandlers.ToList(), preEventAsyncHandlers.ToList());

        public void AddPostEventAsyncHandler(Func<EntityHookEvent<T>[], HookEventType, Task> handler)
            => postEventAsyncHandlers.Add(handler);
        public void AddPreEventAsyncHandler(Func<EntityHookEvent<T>[], HookEventType, Task> handler)
            => preEventAsyncHandlers.Add(handler);

        public async Task InvokePreInsertAsync(EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in preEventAsyncHandlers)
                await a(data, HookEventType.Insert);
        }
        public async Task InvokePostInsertAsync(EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in postEventAsyncHandlers)
                await a(data, HookEventType.Insert);
        }
        public async Task InvokePreDeleteAsync(EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in preEventAsyncHandlers)
                await a(data, HookEventType.Delete);
        }
        public async Task InvokePostDeleteAsync(EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in postEventAsyncHandlers)
                await a(data, HookEventType.Delete);
        }
        public async Task InvokePreUpdateAsync(EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in preEventAsyncHandlers)
                await a(data, HookEventType.Update);
        }
        public async Task InvokePostUpdateAsync(EntityHookEvent[] entities)
        {
            var data = entities.Select(entity => entity.ToTyped<T>()).ToArray();
            foreach (var a in postEventAsyncHandlers)
                await a(data, HookEventType.Update);
        }
    }
}