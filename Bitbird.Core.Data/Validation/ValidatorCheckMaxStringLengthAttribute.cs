using System;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckMaxStringLengthAttribute : PropertyValidatorAttribute
    {
        public int MaxLength { get; }

        [JsonConstructor]
        public ValidatorCheckMaxStringLengthAttribute(int maxLength)
        {
            MaxLength = maxLength;
        }
    }
}
