namespace Bitbird.Core.Data.Net.DbContext.Hooks
{
    public interface IDataContextEntityHookEvents
    {
        void InvokePreInsert(object entity);
        void InvokePostInsert(object entity);
        void InvokePreDelete(object entity);
        void InvokePostDelete(object entity);
        void InvokePreUpdate(object entity);
        void InvokePostUpdate(object entity);
    }
}