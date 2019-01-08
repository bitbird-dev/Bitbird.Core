using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookCollection
    {
        private readonly Dictionary<Type, IDataContextEntityHookEvents> entityHooksEvents;
        private readonly DataContextEntityHookEvents<object> generalHookEvents;

        public DataContextHookCollection() 
            : this (new Dictionary<Type, IDataContextEntityHookEvents>(),
                    new DataContextEntityHookEvents<object>())
        {
        }
        private DataContextHookCollection(Dictionary<Type, IDataContextEntityHookEvents> entityHooksEvents, DataContextEntityHookEvents<object> generalHookEvents)
        {
            this.entityHooksEvents = entityHooksEvents;
            this.generalHookEvents = generalHookEvents;
        }
        
        public DataContextHookCollection Clone() 
            => new DataContextHookCollection(entityHooksEvents.ToDictionary(x => x.Key, x => x.Value.Clone()), generalHookEvents.Clone());

        public DataContextEntityHookEvents<T> ForEntity<T>()
            where T : class
        {
            DataContextEntityHookEvents<T> tHooks;

            if (entityHooksEvents.TryGetValue(typeof(T), out var hooks))
            {
                if (hooks != null)
                    return (DataContextEntityHookEvents<T>)hooks;

                entityHooksEvents[typeof(T)] = tHooks = new DataContextEntityHookEvents<T>();
                return tHooks;
            }

            entityHooksEvents.Add(typeof(T), tHooks = new DataContextEntityHookEvents<T>());
            return tHooks;
        }
        public DataContextEntityHookEvents<object> ForAll()
        {
            return generalHookEvents;
        }
        private IDataContextEntityHookEvents ResolveHooksEventsForType(Type t)
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
        private async Task VisitHookEventsAsync<T>(T[] entities, Func<IDataContextEntityHookEvents, T[], Task> visitAsync)
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

        internal Task InvokePreInsertAsync(EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePreInsertAsync(typedEntities));
        internal Task InvokePostInsertAsync(EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePostInsertAsync(typedEntities));
        internal Task InvokePreDeleteAsync(EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePreDeleteAsync(typedEntities));
        internal Task InvokePostDeleteAsync(EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePostDeleteAsync(typedEntities));
        internal Task InvokePreUpdateAsync(EntityHookEvent[] entitiesHookEvent) => VisitHookEventsAsync(entitiesHookEvent, async (hookEvents, typedEntities) => await hookEvents.InvokePreUpdateAsync(typedEntities));
        internal Task InvokePostUpdateAsync(EntityHookEvent[] entitiesHookEvent) => VisitHookEventsAsync(entitiesHookEvent, async (hookEvents, typedEntities) => await hookEvents.InvokePostUpdateAsync(typedEntities));
    }
}