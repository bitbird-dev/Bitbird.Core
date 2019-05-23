using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckLessThanEqualAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly object Limit;

        public ValidatorCheckLessThanEqualAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}