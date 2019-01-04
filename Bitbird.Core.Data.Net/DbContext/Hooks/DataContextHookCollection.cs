using System;
using System.Collections.Generic;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextHookCollection
    {
        private readonly Dictionary<Type, IDataContextEntityHookEvents> entityHooksEvents = new Dictionary<Type, IDataContextEntityHookEvents>();
        public bool HasHooks => entityHooksEvents.Count != 0;

        public DataContextEntityHookEvents<T> ForEntity<T>()
        {
            if (entityHooksEvents.TryGetValue(typeof(T), out var hooks))
                return (DataContextEntityHookEvents<T>) hooks;

            var thooks = new DataContextEntityHookEvents<T>();
            entityHooksEvents.Add(typeof(T), thooks);
            return thooks;
        }

        internal void InvokePreInsert(object entity)
        {
            if (entityHooksEvents.TryGetValue(entity.GetType(), out var hookEvents))
                hookEvents.InvokePreInsert(entity);
        }
        internal void InvokePostInsert(object entity)
        {
            if (entityHooksEvents.TryGetValue(entity.GetType(), out var hookEvents))
                hookEvents.InvokePostInsert(entity);
        }
        internal void InvokePreDelete(object entity)
        {
            if (entityHooksEvents.TryGetValue(entity.GetType(), out var hookEvents))
                hookEvents.InvokePreDelete(entity);
        }
        internal void InvokePostDelete(object entity)
        {
            if (entityHooksEvents.TryGetValue(entity.GetType(), out var hookEvents))
                hookEvents.InvokePostDelete(entity);
        }
        internal void InvokePreUpdate(object entity)
        {
            if (entityHooksEvents.TryGetValue(entity.GetType(), out var hookEvents))
                hookEvents.InvokePreUpdate(entity);
        }
        internal void InvokePostUpdate(object entity)
        {
            if (entityHooksEvents.TryGetValue(entity.GetType(), out var hookEvents))
                hookEvents.InvokePostUpdate(entity);
        }
    }
}