using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookCollection<TDataContext>
        where TDataContext : System.Data.Entity.DbContext
    {
        private readonly Dictionary<Type, IDataContextEntityHookEvents<TDataContext>> entityHooksEvents;
        private readonly DataContextEntityHookEvents<TDataContext, object> generalHookEvents;

        public DataContextHookCollection() 
            : this (new Dictionary<Type, IDataContextEntityHookEvents<TDataContext>>(),
                    new DataContextEntityHookEvents<TDataContext, object>())
        {
        }
        private DataContextHookCollection(Dictionary<Type, IDataContextEntityHookEvents<TDataContext>> entityHooksEvents, DataContextEntityHookEvents<TDataContext, object> generalHookEvents)
        {
            this.entityHooksEvents = entityHooksEvents;
            this.generalHookEvents = generalHookEvents;
        }
        
        public DataContextHookCollection<TDataContext> Clone() 
            => new DataContextHookCollection<TDataContext>(entityHooksEvents.ToDictionary(x => x.Key, x => x.Value.Clone()), generalHookEvents.Clone());

        public DataContextEntityHookEvents<TDataContext, T> ForEntity<T>()
            where T : class
        {
            DataContextEntityHookEvents<TDataContext, T> tHooks;

            if (entityHooksEvents.TryGetValue(typeof(T), out var hooks))
            {
                if (hooks != null)
                    return (DataContextEntityHookEvents<TDataContext, T>)hooks;

                entityHooksEvents[typeof(T)] = tHooks = new DataContextEntityHookEvents<TDataContext, T>();
                return tHooks;
            }

            entityHooksEvents.Add(typeof(T), tHooks = new DataContextEntityHookEvents<TDataContext, T>());
            return tHooks;
        }
        public DataContextEntityHookEvents<TDataContext, object> ForAll()
        {
            return generalHookEvents;
        }
        private IDataContextEntityHookEvents<TDataContext> ResolveHooksEventsForType(Type t)
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
        private async Task VisitHookEventsAsync<T>(T[] entities, Func<IDataContextEntityHookEvents<TDataContext>, T[], Task> visitAsync)
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

        internal Task InvokePreInsertAsync(TDataContext db, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePreInsertAsync(db, typedEntities));
        internal Task InvokePostInsertAsync(TDataContext db, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePostInsertAsync(db, typedEntities));
        internal Task InvokePreDeleteAsync(TDataContext db, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePreDeleteAsync(db, typedEntities));
        internal Task InvokePostDeleteAsync(TDataContext db, EntityHookEvent[] entities) => VisitHookEventsAsync(entities, async (hookEvents, typedEntities) => await hookEvents.InvokePostDeleteAsync(db, typedEntities));
        internal Task InvokePreUpdateAsync(TDataContext db, EntityHookEvent[] entitiesHookEvent) => VisitHookEventsAsync(entitiesHookEvent, async (hookEvents, typedEntities) => await hookEvents.InvokePreUpdateAsync(db, typedEntities));
        internal Task InvokePostUpdateAsync(TDataContext db, EntityHookEvent[] entitiesHookEvent) => VisitHookEventsAsync(entitiesHookEvent, async (hookEvents, typedEntities) => await hookEvents.InvokePostUpdateAsync(db, typedEntities));
    }
}