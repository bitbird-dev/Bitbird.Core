using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Net.DbContext
{
    /// <summary>
    /// A db model that supports <see href="https://docs.microsoft.com/en-us/ef/ef6/saving/concurrency">optimistic locking</see>.
    /// </summary>
    public interface IOptimisticLockable
    {
        /// <summary>
        /// Identifies the object's version.
        /// Can be used for <see href="https://docs.microsoft.com/en-us/ef/ef6/saving/concurrency">optimistic locking</see>.
        /// </summary>
        [UsedImplicitly]
        DateTime SysStartTime { get; set; }
    }
}