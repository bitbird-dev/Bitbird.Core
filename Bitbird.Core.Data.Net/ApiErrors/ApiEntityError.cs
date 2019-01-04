namespace Bitbird.Core.Data.Net
{
    public class ApiEntityError<TEntity> : ApiError
    {
        public ApiEntityError(string detailMessage)
            : base(ApiErrorType.InvalidEntity, "Entity Error", detailMessage)
        {
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(TEntity)}: {typeof(TEntity).Name}";
        }
    }
}