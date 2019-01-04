namespace Bitbird.Core.Data.Net
{
    public class ApiUserAuthInactiveError : ApiError
    {
        public ApiUserAuthInactiveError(string reason = null)
            : base(ApiErrorType.AuthenticationIsNotActive, "Authentication inactive", $"This authentication method is inactive (Reason: {reason ?? "no details available"}).")
        {
        }
    }
}