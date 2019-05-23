using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckGreaterThanEqualAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly object Limit;

        public ValidatorCheckGreaterThanEqualAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}