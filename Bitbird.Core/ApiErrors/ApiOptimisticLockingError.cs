namespace Bitbird.Core
{
    public class ApiOptimisticLockingError<TEntity> : ApiError
    {
        public ApiOptimisticLockingError(string detailMessage)
            : base(ApiErrorType.OptimisticLocking, "Optimistic Locking Error", detailMessage)
        {
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(TEntity)}: {typeof(TEntity).Name}";
        }
    }
}