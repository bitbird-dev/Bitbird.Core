using System;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiOptimisticLockingError<TEntity> : ApiError
    {
        public ApiOptimisticLockingError([NotNull] string detailMessage)
            : base(ApiErrorType.OptimisticLocking, ApiErrorMessages.ApiOptimisticLockingError_Title, detailMessage)
        {
            if (string.IsNullOrWhiteSpace(detailMessage))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(detailMessage));
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(TEntity)}: {typeof(TEntity).Name}";
        }
    }
}