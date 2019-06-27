using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckGreaterThanEqualAttribute : PropertyValidatorAttribute
    {
        [NotNull] public object Limit { get; }

        [JsonConstructor]
        public ValidatorCheckGreaterThanEqualAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}