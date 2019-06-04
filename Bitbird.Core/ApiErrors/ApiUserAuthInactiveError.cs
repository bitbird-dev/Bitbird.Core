using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [UsedImplicitly]
    public class ApiUserAuthInactiveError : ApiError
    {
        public ApiUserAuthInactiveError([CanBeNull] string reason = null)
            : base(ApiErrorType.AuthenticationIsNotActive, ApiErrorMessages.ApiUserAuthInactiveError_Title,
                string.Format(ApiErrorMessages.ApiUserAuthInactiveError_Message,
                    reason ?? ApiErrorMessages.ApiUserAuthInactiveError_Message_NoReason))
        {
        }
    }
}