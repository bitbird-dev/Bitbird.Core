using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.CliToolAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModelRelationAttribute : Attribute
    {
        public readonly ModelRelationType RelationType;
        [NotNull] public readonly string RelationName;
        public readonly bool IsId;
        [CanBeNull] public readonly string RelatedPropertyName;

        public ModelRelationAttribute(ModelRelationType relationType, [NotNull] string relationName, bool isId, [CanBeNull] string relatedPropertyName = null)
        {
            if (!Enum.IsDefined(typeof(ModelRelationType), relationType))
                throw new InvalidEnumArgumentException(nameof(relationType), (int) relationType, typeof(ModelRelationType));

            if (string.IsNullOrWhiteSpace(relationName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(relationName));

            RelationType = relationType;
            RelationName = relationName;
            IsId = isId;
            RelatedPropertyName = relatedPropertyName;
        }
    }
}