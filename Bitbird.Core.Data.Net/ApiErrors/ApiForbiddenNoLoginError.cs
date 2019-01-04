namespace Bitbird.Core.Data.Net
{
    public class ApiForbiddenNoLoginError : ApiError
    {
        public ApiForbiddenNoLoginError()
            : base(ApiErrorType.ForbiddenNoLogin, "Not logged in", "The current session is not logged in.")
        {
        }
    }
}