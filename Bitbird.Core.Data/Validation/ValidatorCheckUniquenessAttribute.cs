using System;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckUniquenessAttribute : PropertyValidatorAttribute
    {
        public readonly bool AmongNonDeletedOnly;
        public readonly bool AmongActiveOnly;

        public ValidatorCheckUniquenessAttribute(bool amongNonDeletedOnly = false, bool amongActiveOnly = false)
        {
            AmongNonDeletedOnly = amongNonDeletedOnly;
            AmongActiveOnly = amongActiveOnly;
        }
    }
}