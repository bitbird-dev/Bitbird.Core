using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace Bitbird.Core.Data
{
    /// <summary>
    /// Stores a token.
    /// Not persisted as own table, but as part of deriving classes' tables.
    /// See <see cref="IToken{TKey}"/>.
    /// Implements <see cref="IToken{TKey}"/>, <see cref="IId{T}"/>.
    /// </summary>
    /// <inheritdoc cref="IToken{T}" />
    public abstract class TokenBase : IToken<long>
    {
        /// <inheritdoc />
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [UsedImplicitly]
        public long Id { get; set; }

        /// <summary>
        /// The token-key.
        /// Uniquely identifies this token among other tokens of the same type.
        /// Cannot be null.
        /// Max-length: 61.
        /// Must be unique.
        /// See <see cref="IToken{T}" />.
        /// </summary>
        /// <inheritdoc cref="IToken{T}" />
        [Required]
        [StringLength(61)]
        [UsedImplicitly, CanBeNull]
        public string TokenKey { get; set; }

        /// <summary>
        /// The timestamp until which this token is valid.
        /// Sql-DataType: datetime2.
        /// </summary>
        /// <inheritdoc cref="IToken{T}" />
        [UsedImplicitly]
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// The timestamp at which this token was created.
        /// Sql-DataType: datetime2.
        /// </summary>
        [UsedImplicitly]
        public DateTime CreationDateTime { get; set; }
    }
}
