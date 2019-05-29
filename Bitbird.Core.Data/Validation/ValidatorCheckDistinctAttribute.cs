using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public interface IDistinctSelectEqualityMemberProvider<in TItem, out TEqualityMember>
    {
        TEqualityMember GetEqualityMember([NotNull] TItem item);
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ValidatorCheckDistinctAttribute : PropertyValidatorAttribute
    {
        [CanBeNull] public readonly Type DistinctSelectEqualityMemberProviderType;

        public ValidatorCheckDistinctAttribute([CanBeNull] Type distinctSelectEqualityMemberProviderType = null)
        {
            DistinctSelectEqualityMemberProviderType = distinctSelectEqualityMemberProviderType;
        }
    }
}