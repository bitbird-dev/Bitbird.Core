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

        public virtual Task BeforeExecution() => Task.FromResult(true);

        protected abstract Task<TData> GetDataAsync();
    }
}