using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CopyValidatorFromAttribute : PropertyValidatorAttribute
    {
        [NotNull] public readonly Dictionary<Type, object[]> CopiedAttributes;

        public CopyValidatorFromAttribute([NotNull] Type type, [NotNull] string propertyName, [NotNull, ItemNotNull] params Type[] ignoreAttributes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));
            if (ignoreAttributes == null)
                throw new ArgumentException("Is null.", nameof(ignoreAttributes));
            if (ignoreAttributes.Any(a => a == null))
                throw new ArgumentException("An element is null.", nameof(ignoreAttributes));

            var property = type.GetProperty(propertyName) ??
                throw new Exception($"A {nameof(CopyValidatorFromAttribute)} was created with Type={type.FullName} and PropertyName={propertyName}. The given type does not have a property with this name.");

            CopiedAttributes = property.GetCustomAttributes(true)
                .SelectMany(attr =>
                {
                    if (ignoreAttributes.Any(i => i.IsInstanceOfType(attr)))
                        return new KeyValuePair<Type, object>[0];

                    AttributeTranslations.TryTranslateAttribute(attr, out var result);

                    return new [] { new KeyValuePair<Type, object>(result.GetType(), result) };
                })
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(group => group.Key, group => group.Select(kvp => kvp.Value).ToArray());
        }
    }
}