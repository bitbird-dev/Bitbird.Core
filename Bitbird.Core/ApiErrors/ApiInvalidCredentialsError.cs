using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [UsedImplicitly]
    public class ApiInvalidCredentialsError : ApiError
    {
        public ApiInvalidCredentialsError(string detailInfo)
            : base(ApiErrorType.CredentialsAreInvalid, 
                ApiErrorMessages.ApiInvalidCredentialsError_Title,
                string.Format(ApiErrorMessages.ApiInvalidCredentialsError_Message, detailInfo))
        {
        }
    }
}