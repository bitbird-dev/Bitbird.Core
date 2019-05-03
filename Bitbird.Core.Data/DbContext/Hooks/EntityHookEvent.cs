using System;
using System.Collections;
using System.Collections.Generic;

namespace Bitbird.Core.Data.DbContext.Hooks
{
    public class EntityHookEvent<T>
    {
        public readonly T OldEntity;
        public readonly T NewEntity;

        public EntityHookEvent(T oldEntity, T newEntity)
        {
            OldEntity = oldEntity;
            NewEntity = newEntity;
        }

        public bool HasChanged<TProp>(Func<T, TProp> propertySelector)
        {
            if (OldEntity == null || NewEntity == null)
                return true;

            var oldValue = propertySelector(OldEntity);
            var newValue = propertySelector(NewEntity);

            if (!EqualityComparer<TProp>.Default.Equals(oldValue, newValue))
                return true;

            return false;
        }
    }

    public class EntityHookEvent
    {
        public readonly object OldEntity;
        public readonly object NewEntity;

        public EntityHookEvent(object oldEntity, object newEntity)
        {
            OldEntity = oldEntity;
            NewEntity = newEntity;
        }

        public EntityHookEvent<T> ToTyped<T>()
            where T : class
        {
            return new EntityHookEvent<T>(OldEntity as T, NewEntity as T);
        }
    }
}