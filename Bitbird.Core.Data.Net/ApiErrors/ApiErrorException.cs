using System;
using System.Linq;

namespace Bitbird.Core.Data.Net
{
    public class ApiErrorException : Exception
    {
        public readonly Exception BaseException;
        public readonly ApiError[] ApiErrors;

        public ApiErrorException(params ApiError[] apiErrors)
            : this(null, apiErrors)
        {
        }
        public ApiErrorException(Exception baseException, params ApiError[] apiErrors)
            : base($"Api errors have occurred (Details: {{ {apiErrors.Select(apiError => $"{{ {apiError} }}").Aggregate((a, b) => $"{a}, {b}")} }}).")
        {
            BaseException = baseException;
            ApiErrors = apiErrors;
        }
    }
}