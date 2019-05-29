using System;

namespace Bitbird.Core.Data.Validation
{
    internal class ModelValidatorKey : IEquatable<ModelValidatorKey>
    {
        public readonly Type EntityType;
        public readonly Type ModelType;

        public ModelValidatorKey(Type entityType, Type modelType)
        {
            EntityType = entityType;
            ModelType = modelType;
        }

        public override string ToString()
        {
            return $"{nameof(EntityType)}: {EntityType}, {nameof(ModelType)}: {ModelType}";
        }

        public bool Equals(ModelValidatorKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(EntityType, other.EntityType) && Equals(ModelType, other.ModelType);
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return Equals((ModelValidatorKey)other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EntityType != null ? EntityType.GetHashCode() : 0) * 397) ^ (ModelType != null ? ModelType.GetHashCode() : 0);
            }
        }
    }
}