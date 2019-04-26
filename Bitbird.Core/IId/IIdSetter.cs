using Bitbird.Core;

namespace Bitbird.Core
{
    /// <inheritdoc />
    public interface IIdSetter<T> : IId<T>
    {
        /// <summary>
        /// See <see cref="IId{T}"/>.
        /// </summary>
        new T Id { get; set; }
    }
}