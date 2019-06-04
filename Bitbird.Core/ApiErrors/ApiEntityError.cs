using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiEntityError<TEntity> : ApiError
    {
        public ApiEntityError([NotNull] string detailMessage)
            : base(ApiErrorType.InvalidEntity, ApiErrorMessages.ApiEntityError_Title, detailMessage)
        {
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(TEntity)}: {typeof(TEntity).Name}";
        }
    }
}