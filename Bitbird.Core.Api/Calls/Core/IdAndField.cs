using JetBrains.Annotations;

namespace Bitbird.Core.Api.Calls.Core
{
    /// <summary>
    /// Stores an id and a typed field.
    /// Commonly used for retrieving structured data from the database using expressions.
    /// </summary>
    /// <typeparam name="TFieldType">The type of the field.</typeparam>
    /// <inheritdoc cref="IId{T}" />
    public class IdAndField<TFieldType> : IId<long>
    {
        /// <summary>
        /// The id.
        /// </summary>
        /// <inheritdoc cref="IId{T}.Id" />
        [UsedImplicitly]
        public long Id { get; set; }

        /// <summary>
        /// The field.
        /// Can be null.
        /// </summary>
        [CanBeNull, UsedImplicitly]
        public TFieldType Field { get; set; }
    }
}
