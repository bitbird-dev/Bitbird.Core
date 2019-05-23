using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckBetweenInclusiveAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly object LowerLimit;
        [NotNull] public readonly object UpperLimit;

        public ValidatorCheckBetweenInclusiveAttribute([NotNull] object lowerLimit, [NotNull] object upperLimit)
        {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }
    }
}