using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckBetweenExclusiveAttribute : PropertyValidatorAttribute
    {
        [NotNull] public object LowerLimit { get; }
        [NotNull] public object UpperLimit { get; }

        [JsonConstructor]
        public ValidatorCheckBetweenExclusiveAttribute([NotNull] object lowerLimit, [NotNull] object upperLimit)
        {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }
    }
}