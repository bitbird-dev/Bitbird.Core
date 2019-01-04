namespace Bitbird.Core.Data.Net
{
    public class ApiInvalidCredentialsError : ApiError
    {
        public ApiInvalidCredentialsError(string detailInfo)
            : base(ApiErrorType.CredentialsAreInvalid, "Invalid credentials", $"The provided credentials are invalid (Details: {detailInfo}).")
        {
        }
    }
}