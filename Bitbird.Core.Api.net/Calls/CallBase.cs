using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net
{
    public interface ICallBase<TData>
    {
        [NotNull, ItemNotNull]
        Task<TData> ExecuteAsync();
    }

    public abstract class CallBase<TData, TPermissionParameter>
        : ICallBase<TData>
        where TPermissionParameter : IPermissionParameter
    {
        public TPermissionParameter PermissionParameter { get; }

        protected CallBase([NotNull]TPermissionParameter permissionParameter)
        {
            PermissionParameter = permissionParameter;
        }

        [NotNull, ItemNotNull]
        public virtual async Task<TData> ExecuteAsync()
        {
            PermissionParameter.CheckIsPermitted();
            await BeforeExecution();
            return await GetDataAsync();
        }

        [NotNull]
        public virtual Task BeforeExecution() => Task.FromResult(true);

        [NotNull, ItemNotNull]
        protected abstract Task<TData> GetDataAsync();



        [NotNull, ContractAnnotation("entry:null => halt")]
        protected T CheckNullClass<T, TCheckId>([CanBeNull] T entry, [CanBeNull] TCheckId id)
            where T : class
            where TCheckId : class
        {
            return entry ?? throw new ApiErrorException(ApiNotFoundError.Create(typeof(T).Name, id));
        }

        [NotNull, ContractAnnotation("entry:null => halt")]
        protected T CheckNullStruct<T, TCheckId>([CanBeNull] T entry, [CanBeNull] TCheckId? id)
            where T : class
            where TCheckId : struct
        {
            return entry ?? throw new ApiErrorException(ApiNotFoundError.Create(typeof(T).Name, id));
        }

        [NotNull, ContractAnnotation("entry:null => halt")]
        protected T CheckNullStruct<T, TCheckId>([CanBeNull] T entry, [CanBeNull] TCheckId id)
            where T : class
            where TCheckId : struct
        {
            return entry ?? throw new ApiErrorException(ApiNotFoundError.Create(typeof(T).Name, id));
        }
    }
}