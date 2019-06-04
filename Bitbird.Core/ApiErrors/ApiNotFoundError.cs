using System;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    [UsedImplicitly]
    public class ApiNotFoundError : ApiError
    {
        [UsedImplicitly]
        public ApiNotFoundError([NotNull] string elementTypeName, [NotNull] string elementIdentifier, [CanBeNull] string identifierInfo = null)
            : base(ApiErrorType.NotFound, 
                ApiErrorMessages.ApiNotFoundError_Title,
                string.Format(
                    ApiErrorMessages.ApiNotFoundError_Message,
                    elementTypeName, elementIdentifier, identifierInfo ?? ApiErrorMessages.ApiNotFoundError_Message_NoIdentifierInfo))
        {
            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(elementTypeName));
            if (string.IsNullOrWhiteSpace(elementIdentifier))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(elementIdentifier));
        }

        [NotNull]
        public static ApiNotFoundError Create<TId>([NotNull] string elementTypeName, [CanBeNull] TId elementIdentifier, [CanBeNull] string identifierInfo = null)
        {
            if (string.IsNullOrWhiteSpace(elementTypeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(elementTypeName));

            return new ApiNotFoundError(elementTypeName, elementIdentifier == null ? "null" : elementIdentifier.ToString(), identifierInfo);
        }
    }
}