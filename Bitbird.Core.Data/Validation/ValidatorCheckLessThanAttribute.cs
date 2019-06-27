using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckLessThanAttribute : PropertyValidatorAttribute
    {
        [NotNull] public object Limit { get; }

        [JsonConstructor]
        public ValidatorCheckLessThanAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}