namespace Bitbird.Core.Api.Models.Base
{
    /// <summary>
    /// An api model that supports <see href="https://docs.microsoft.com/en-us/ef/ef6/saving/concurrency">optimistic locking</see>.
    /// </summary>
    public interface IOptimisticLockableModel
    {
        /// <summary>
        /// Stores the encoded object's version.
        /// May store various versions of related db objects that are consolidated in this api model.
        /// Can be used for <see href="https://docs.microsoft.com/en-us/ef/ef6/saving/concurrency">optimistic locking</see>.
        /// </summary>
        string OptimisticLockingToken { get; set; }
    }
}