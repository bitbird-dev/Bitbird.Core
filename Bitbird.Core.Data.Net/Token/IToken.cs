using System;

namespace Bitbird.Core.Data.Net
{
    /// <summary>
    /// An object that can be used as token.
    /// Tokens usually contain an identifier (<see cref="IId{T}.Id"/>) and a key (<see cref="TokenKey"/>).
    /// Both of them uniquely identify the token among other tokens of the same type, but <see cref="TokenKey"/> stores additional information
    /// (e.g. is cryptically signed).
    /// Tokens are not valid forever, see <see cref="ValidUntil"/>.
    /// Implements <see cref="IId{TKey}"/>.
    /// </summary>
    /// <typeparam name="TKey">The data-type of the token's <see cref="IId{T}.Id"/>. See <see cref="IId{T}"/></typeparam>
    public interface IToken<out TKey> : IId<TKey>
    {
        /// <summary>
        /// The token-key.
        /// Uniquely identifies this token among other tokens of the same type.
        /// </summary>]
        string TokenKey { get; }

        /// <summary>
        /// The timestamp until which this token is valid.
        /// </summary>
        DateTime ValidUntil { get; }
    }
}