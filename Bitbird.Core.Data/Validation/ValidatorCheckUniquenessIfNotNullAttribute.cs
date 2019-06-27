using System;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckUniquenessIfNotNullAttribute : PropertyValidatorAttribute
    {
        public bool AmongNonDeletedOnly { get; }
        public bool AmongActiveOnly { get; }

        [JsonConstructor]
        public ValidatorCheckUniquenessIfNotNullAttribute(bool amongNonDeletedOnly = false, bool amongActiveOnly = false)
        {
            AmongNonDeletedOnly = amongNonDeletedOnly;
            AmongActiveOnly = amongActiveOnly;
        }
    }
}