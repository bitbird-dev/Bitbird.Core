namespace Bitbird.Core
{
    public class ApiUserIsInActiveError : ApiError
    {
        public ApiUserIsInActiveError()
            : base(ApiErrorType.UserIsLocked, "User is inactive", "The user is currently inactive. Contact your administrator.")
        {
        }
    }
}