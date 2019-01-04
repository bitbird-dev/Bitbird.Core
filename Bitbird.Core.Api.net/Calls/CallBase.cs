using System.Threading.Tasks;

namespace Bitbird.Core.Api.Net
{
    public abstract class CallBase<TData, TPermissionParameter>
        where TPermissionParameter : IPermissionValidation
    {
        public readonly TPermissionParameter PermissionParameter;

        protected CallBase(TPermissionParameter permissionParameter)
        {
            PermissionParameter = permissionParameter;
        }

        public virtual async Task<TData> ExecuteAsync()
        {
            PermissionParameter.CheckIsPermitted();
            await BeforeExecution();
            return await GetDataAsync();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task BeforeExecution() { }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        protected abstract Task<TData> GetDataAsync();
    }

}
