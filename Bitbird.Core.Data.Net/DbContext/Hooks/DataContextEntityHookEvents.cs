using System;

namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public class DataContextEntityHookEvents<T> : IDataContextEntityHookEvents
    {
        public event Action<T> PreInsert;
        public event Action<T> PostInsert;
        public event Action<T> PreDelete;
        public event Action<T> PostDelete;
        public event Action<T> PreUpdate;
        public event Action<T> PostUpdate;

        public void InvokePreInsert(object entity) => PreInsert?.Invoke((T)entity);
        public void InvokePostInsert(object entity) => PostInsert?.Invoke((T)entity);
        public void InvokePreDelete(object entity) => PreDelete?.Invoke((T)entity);
        public void InvokePostDelete(object entity) => PostDelete?.Invoke((T)entity);
        public void InvokePreUpdate(object entity) => PreUpdate?.Invoke((T)entity);
        public void InvokePostUpdate(object entity) => PostUpdate?.Invoke((T)entity);
    }
}