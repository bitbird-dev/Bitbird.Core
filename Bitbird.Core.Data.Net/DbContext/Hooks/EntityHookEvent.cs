namespace Bitbird.Core.Data.Net.DbContext.Hooks
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