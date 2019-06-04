using System;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiAttributeMustBeUniqueError : ApiAttributeError
    {
        [CanBeNull, UsedImplicitly]
        public readonly string Value;

        public ApiAttributeMustBeUniqueError([NotNull] string attributeName, [CanBeNull] string value)
            : base(attributeName, string.Format(ApiErrorMessages.ApiAttributeMustBeUniqueError_Title, attributeName, value), ApiErrorType.MustBeUnique)
        {
            if (string.IsNullOrEmpty(attributeName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(attributeName));

            Value = value;
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(Value)}: {Value}";
        }
    }
}