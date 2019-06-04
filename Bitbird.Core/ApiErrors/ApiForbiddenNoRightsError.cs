using System;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiForbiddenNoRightsError : ApiError
    {
        public ApiForbiddenNoRightsError([NotNull] string actionDescription)
            : base(ApiErrorType.ForbiddenNoRights, ApiErrorMessages.ApiForbiddenNoRightsError_Title,
                string.Format(
                    ApiErrorMessages.ApiForbiddenNoRightsError_Message,
                    actionDescription))
        {
            if (string.IsNullOrWhiteSpace(actionDescription))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(actionDescription));
        }
    }
}