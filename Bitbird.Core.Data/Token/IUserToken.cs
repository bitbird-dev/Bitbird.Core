namespace Bitbird.Core.Data
{
    /// <summary>
    /// An object that stores a user-specific token.
    /// For more information see <see cref="IToken{TKey}"/>, <see cref="IId{T}"/>.
    /// Implements <see cref="IToken{TKey}"/>.
    /// </summary>
    /// <typeparam name="TUserKey">The data-type of <see cref="UserId"/>. Simple types are recommended.</typeparam>
    /// <typeparam name="TTokenKey">The data-type of the token's <see cref="IId{T}.Id"/>. See <see cref="IId{T}"/></typeparam>
    public interface IUserToken<out TUserKey, out TTokenKey> : IToken<TTokenKey>
    {
        /// <summary>
        /// The user's id.
        /// Should not be null.
        /// </summary>
        TUserKey UserId { get; }
    }
}