using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckLessThanAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly object Limit;

        public ValidatorCheckLessThanAttribute([NotNull] object limit)
        {
            Limit = limit;
        }
    }
}