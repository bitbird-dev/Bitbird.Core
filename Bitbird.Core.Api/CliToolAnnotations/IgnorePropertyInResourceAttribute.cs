using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.CliToolAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class IgnorePropertyInResourceAttribute : Attribute
    {
        [NotNull] public readonly string PropertyName;

        public IgnorePropertyInResourceAttribute([NotNull] string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            PropertyName = propertyName;
        }
    }
}