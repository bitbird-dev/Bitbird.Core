using System;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckMaxStringLengthAttribute : PropertyValidatorAttribute
    {
        public readonly int MaxLength;

        public ValidatorCheckMaxStringLengthAttribute(int maxLength)
        {
            MaxLength = maxLength;
        }
    }
}
