using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckBetweenExclusiveAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly object LowerLimit;
        [NotNull] public readonly object UpperLimit;

        public ValidatorCheckBetweenExclusiveAttribute([NotNull] object lowerLimit, [NotNull] object upperLimit)
        {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }
    }
}