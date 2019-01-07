namespace Bitbird.Core
{
    public class ApiUserIsLockedError : ApiError
    {
        public ApiUserIsLockedError()
            : base(ApiErrorType.UserIsLocked, "User is locked", "The user is currently locked. Contact your administrator.")
        {
        }
    }
}