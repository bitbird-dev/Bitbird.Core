using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckGreaterThanAttribute : PropertyValidatorAttribute
    {
        [NotNull] public object Limit { get; }

        [JsonConstructor]
        public ValidatorCheckGreaterThanAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}