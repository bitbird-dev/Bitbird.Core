using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [UsedImplicitly]
    public class ApiForbiddenNoLoginError : ApiError
    {
        public ApiForbiddenNoLoginError()
            : base(ApiErrorType.ForbiddenNoLogin, ApiErrorMessages.ApiForbiddenNoLoginError_Title, ApiErrorMessages.ApiForbiddenNoLoginError_Message)
        {
        }
    }
}