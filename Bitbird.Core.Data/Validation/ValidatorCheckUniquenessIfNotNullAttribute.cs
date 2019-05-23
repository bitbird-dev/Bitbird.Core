using System;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckUniquenessIfNotNullAttribute : PropertyValidatorAttribute
    {
        public readonly bool AmongNonDeletedOnly;
        public readonly bool AmongActiveOnly;

        public ValidatorCheckUniquenessIfNotNullAttribute(bool amongNonDeletedOnly = false, bool amongActiveOnly = false)
        {
            AmongNonDeletedOnly = amongNonDeletedOnly;
            AmongActiveOnly = amongActiveOnly;
        }
    }
}