using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.DbContext.Hooks
{
    public class DataContextHookCollection<TDataContext, TState>
    {
        private readonly Dictionary<Type, IDataContextEntityHookEvents<TDataContext, TState>> entityHooksEvents;
        private readonly DataContextEntityHookEvents<TDataContext, object, TState> generalHookEvents;

        public DataContextHookCollection() 
            : this (new Dictionary<Type, IDataContextEntityHookEvents<TDataContext, TState>>(),
                    new DataContextEntityHookEvents<TDataContext, object, TState>())
        {
        }
        private DataContextHookCollection(Dictionary<Type, IDataContextEntityHookEvents<TDataContext, TState>> entityHooksEvents, DataContextEntityHookEvents<TDataContext, object, TState> generalHookEvents)
        {
            this.entityHooksEvents = entityHooksEvents;
            this.generalHookEvents = generalHookEvents;
        }
        
        public DataContextHookCollection<TDataContext, TState> Clone() 
            => new DataContextHookCollection<TDataContext, TState>(
                entityHooksEvents.ToDictionary(x => x.Key, x => x.Value.Clone()), 
                generalHookEvents.Clone());

        public DataContextEntityHookEvents<TDataContext, T, TState> ForEntity<T>()
            where T : class
        {
            DataContextEntityHookEvents<TDataContext, T, TState> tHooks;

            if (entityHooksEvents.TryGetValue(typeof(T), out var hooks))
            {
                if (hooks != null)
                    return (DataContextEntityHookEvents<TDataContext, T, TState>)hooks;

                entityHooksEvents[typeof(T)] = tHooks = new DataContextEntityHookEvents<TDataContext, T, TState>();
                return tHooks;
            }

            entityHooksEvents.Add(typeof(T), tHooks = new DataContextEntityHookEvents<TDataContext, T, TState>());
            return tHooks;
        }
        public DataContextEntityHookEvents<TDataContext, object, TState> ForAll()
        {
            return generalHookEvents;
        }
        private IDataContextEntityHookEvents<TDataContext, TState> ResolveHooksEventsForType(Type t)
        {
            if (entityHooksEvents.TryGetValue(t, out var hookEvents))
                return hookEvents;

            hookEvents = entityHooksEvents
                .Where(registeredType => registeredType.Key.IsAssignableFrom(t))
                .Select(kvp => kvp.Value)
                .FirstOrDefault();

            entityHooksEvents[t] = hookEvents;
            return hookEvents;
        }
        private async Task VisitHookEventsAsync<T>(T[] entities, Func<IDataContextEntityHookEvents<TDataContext, TState>, T[], Task> visitAsync)
        {
            Func<T, Type> typeSelector = t => t.GetType();
            if (typeof(T) == typeof(EntityHookEvent))
                typeSelector = t =>
                {
                    var ehe = t as EntityHookEvent;
                    return (ehe?.NewEntity ?? ehe?.OldEntity)?.GetType();
                };

            var byType = entities
                .GroupBy(typeSelector)
                .Select(group => new
                {
                    HookEvents = ResolveHooksEventsForType(group.Key),
                    Entities = group.ToArray()
                })
                .Where(type => type.HookEvents != null)
                .ToList();

            foreach (var type in byType)
                await visitAsync(type.HookEvents, type.Entities);

            await visitAsync(generalHookEvents, entities);
        }

        public Task InvokePreInsertAsync(TDataContext db, TState state, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePreInsertAsync(db, state, typedEntities));
        public Task InvokePostInsertAsync(TDataContext db, TState state, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePostInsertAsync(db, state, typedEntities));
        public Task InvokePreDeleteAsync(TDataContext db, TState state, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePreDeleteAsync(db, state, typedEntities));
        public Task InvokePostDeleteAsync(TDataContext db, TState state, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePostDeleteAsync(db, state, typedEntities));
        public Task InvokePreUpdateAsync(TDataContext db, TState state, EntityHookEvent[] entitiesHookEvent) => VisitHookEventsAsync(entitiesHookEvent, async (hookEvents, typedEntities) => await hookEvents.InvokePreUpdateAsync(db, state, typedEntities));
        public Task InvokePostUpdateAsync(TDataContext db, TState state, EntityHookEvent[] entitiesHookEvent) => VisitHookEventsAsync(entitiesHookEvent, async (hookEvents, typedEntities) => await hookEvents.InvokePostUpdateAsync(db, state, typedEntities));
    }
}