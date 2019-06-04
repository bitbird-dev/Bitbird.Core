using System;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [UsedImplicitly]
    public class ApiPreconditionError : ApiError
    {
        public ApiPreconditionError([NotNull] string violationDescription)
            : base(ApiErrorType.PreconditionViolation, ApiErrorMessages.ApiPreconditionError_Title, violationDescription)
        {
            if (string.IsNullOrWhiteSpace(violationDescription))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(violationDescription));
        }
    }
}