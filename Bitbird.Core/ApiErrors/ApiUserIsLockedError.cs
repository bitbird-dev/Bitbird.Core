using Bitbird.Core.ApiErrors;

namespace Bitbird.Core
{
    public class ApiUserIsLockedError : ApiError
    {
        public ApiUserIsLockedError()
            : base(ApiErrorType.UserIsLocked, ApiErrorMessages.ApiUserIsLockedError_Title, ApiErrorMessages.ApiUserIsLockedError_Message)
        {
        }
    }
}