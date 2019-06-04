using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [UsedImplicitly]
    public class ApiUserIsInActiveError : ApiError
    {
        public ApiUserIsInActiveError()
            : base(ApiErrorType.UserIsLocked, ApiErrorMessages.ApiUserIsInActiveError_Title, ApiErrorMessages.ApiUserIsInActiveError_Message)
        {
        }
    }
}