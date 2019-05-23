using System;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckNotNullOrEmptyAttribute : PropertyValidatorAttribute
    {
    }
}