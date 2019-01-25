namespace Bitbird.Core
{
    /// <summary>
    /// An object that can be uniquely identified among other objects of the same type by an id.
    /// </summary>
    /// <typeparam name="T">The id's data-type. Simple types are recommended.</typeparam>
    public interface IId<out T>
    {
        /// <summary>
        /// An id that uniquely identifies this object among other objects of the same type.
        /// </summary>
        T Id { get; }
    }
}
