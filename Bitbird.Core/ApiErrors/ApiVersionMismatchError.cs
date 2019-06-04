using Bitbird.Core.ApiErrors;

namespace Bitbird.Core
{
    public class ApiVersionMismatchError : ApiError
    {
        private readonly long serverVersion;
        private readonly long clientVersion;

        public ApiVersionMismatchError(long serverVersion, long clientVersion)
            : base(ApiErrorType.ApiVersionMismatch, 
                ApiErrorMessages.ApiVersionMismatchError_Title,
                string.Format(
                    ApiErrorMessages.ApiVersionMismatchError_Message,
                    clientVersion, serverVersion))
        {
            this.serverVersion = serverVersion;
            this.clientVersion = clientVersion;
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(serverVersion)}: {serverVersion}; {nameof(clientVersion)}: {clientVersion}";
        }
    }
}