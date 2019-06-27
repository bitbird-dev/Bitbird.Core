using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckBetweenInclusiveAttribute : PropertyValidatorAttribute
    {
        [NotNull] public object LowerLimit { get; }
        [NotNull] public object UpperLimit { get; }

        [JsonConstructor]
        public ValidatorCheckBetweenInclusiveAttribute([NotNull] object lowerLimit, [NotNull] object upperLimit)
        {
            LowerLimit = lowerLimit;
            UpperLimit = upperLimit;
        }
    }
}