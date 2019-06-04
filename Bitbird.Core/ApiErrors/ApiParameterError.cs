using System;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiParameterError : ApiError
    {
        [NotNull]
        public readonly string ParameterName;

        public ApiParameterError([NotNull] string parameterName, [NotNull] string detailMessage) 
            : base(ApiErrorType.InvalidParameter, ApiErrorMessages.ApiParameterError_Title, detailMessage)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parameterName));
            if (string.IsNullOrWhiteSpace(detailMessage))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(detailMessage));

            ParameterName = parameterName;
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(ParameterName)}: {ParameterName}";
        }
    }
}