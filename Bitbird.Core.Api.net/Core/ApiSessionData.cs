using Bitbird.Core.Api.Net.Properties;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Core
{
    /// <summary>
    /// Low-level session data.
    /// Stores data directly transmitted by the client.
    /// The most important member is LoginTokenKey which is the token-key of the current session. If it is null or whitespace, the current session is not logged in.
    /// </summary>
    public class ApiSessionData
    {
        [CanBeNull] public readonly string LoginTokenKey;
        [NotNull] public readonly string ClientApplicationIdentifier;

        public ApiSessionData(
            [CanBeNull] string loginTokenKey, 
            [CanBeNull] string clientApplicationIdentifier)
        {
            if (string.IsNullOrWhiteSpace(clientApplicationIdentifier) || clientApplicationIdentifier.Length > 5)
                throw new ApiErrorException(new ApiParameterError(nameof(ClientApplicationIdentifier), string.Format(Resources.Core_ApiSessionData_ClientApplicationId, 5)));

            LoginTokenKey = loginTokenKey;
            ClientApplicationIdentifier = clientApplicationIdentifier;
        }
    }
}