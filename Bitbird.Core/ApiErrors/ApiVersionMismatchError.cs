namespace Bitbird.Core
{
    public class ApiVersionMismatchError : ApiError
    {
        private readonly long serverVersion;
        private readonly long clientVersion;

        public ApiVersionMismatchError(long serverVersion, long clientVersion)
            : base(ApiErrorType.ApiVersionMismatch, "Api Version Mismatch", $"The requested interface version is not supported (Requested version: {clientVersion}, Supported version: {serverVersion}).")
        {
            this.serverVersion = serverVersion;
            this.clientVersion = clientVersion;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(serverVersion)}: {serverVersion}, {nameof(clientVersion)}: {clientVersion}";
        }
    }
}