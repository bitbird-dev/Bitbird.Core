using System;
using System.Linq;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiErrorException : Exception
    {
        [NotNull, ItemNotNull] public readonly ApiError[] ApiErrors;

        public ApiErrorException([NotNull, ItemNotNull] params ApiError[] apiErrors)
            : base(FormatMessage(apiErrors))
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (apiErrors == null) throw new ArgumentNullException(nameof(apiErrors));
            if (apiErrors.Any(x => x == null)) throw new ArgumentNullException(nameof(apiErrors));

            ApiErrors = apiErrors;
        }
        public ApiErrorException([NotNull] Exception innerException, [NotNull, ItemNotNull] params ApiError[] apiErrors)
            : base(FormatMessage(apiErrors), innerException)
        {
            if (innerException == null) throw new ArgumentNullException(nameof(innerException));
            // ReSharper disable once JoinNullCheckWithUsage
            if (apiErrors == null) throw new ArgumentNullException(nameof(apiErrors));
            if (apiErrors.Any(x => x == null)) throw new ArgumentNullException(nameof(apiErrors));

            ApiErrors = apiErrors;
        }

        [NotNull]
        private static string FormatMessage([NotNull, ItemNotNull] ApiError[] apiErrors)
        {
            if (apiErrors == null) throw new ArgumentNullException(nameof(apiErrors));
            if (apiErrors.Any(x => x == null)) throw new ArgumentNullException(nameof(apiErrors));

            return string.Format(
                ApiErrorMessages.ApiErrorException_Message,
                apiErrors
                    .Select(apiError => $"{{ {apiError} }}")
                    .Aggregate((a, b) => $"{a}, {b}"));
        }
    }
}