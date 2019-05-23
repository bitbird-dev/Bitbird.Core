using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckGreaterThanAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly object Limit;

        public ValidatorCheckGreaterThanAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}