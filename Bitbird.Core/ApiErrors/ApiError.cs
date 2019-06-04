using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiError
    {
        public readonly ApiErrorType Type;
        [NotNull]
        public readonly string Title;
        [NotNull]
        public readonly string DetailMessage;

        [UsedImplicitly]
        public ApiError(ApiErrorType type, [NotNull] string title, [NotNull] string detailMessage)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(title));
            if (string.IsNullOrWhiteSpace(detailMessage))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(detailMessage));
            if (!Enum.IsDefined(typeof(ApiErrorType), type))
                throw new InvalidEnumArgumentException(nameof(type), (int) type, typeof(ApiErrorType));

            Type = type;
            Title = title;
            DetailMessage = detailMessage;
        }

        public override string ToString()
        {
            return $"[{Type}]: {Title} ({DetailMessage})";
        }
    }
}