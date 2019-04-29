using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net.Models.Base
{
    /// <summary>
    /// Provides methods to convert optimistic locking fields to tokens and back.
    /// </summary>
    public static class OptimisticLockingHelper
    {
        /// <summary>
        /// Currently we use datetime2(0) for our optimistic locking fields.
        /// datetime2(0) does not store anything smaller than seconds.
        /// Ticks are stored in 1/10000000 of a second.
        ///
        /// Therefore we can ignore the last log(10000000,10) digits without loosing information,
        /// meanwhile saving storage space (since the result is converted to a string).
        /// </summary>
        private const long IgnoredTicks = 10000000L;

        /// <summary>
        /// Returns a string representation of the given optimistic locking field.
        /// Inverse: <see cref="TokenToDateTime"/>.
        /// </summary>
        /// <param name="dt">The optimistic locking field.</param>
        /// <returns>A string representation of the locking token.</returns>
        [NotNull]
        public static string DateTimeToToken(DateTime dt) 
            => Convert.ToString(dt.Ticks / IgnoredTicks, 16);

        /// <summary>
        /// Returns the locking token as datetime from a given string representation.
        /// Inverse: <see cref="DateTimeToToken"/>.
        /// </summary>
        /// <param name="token">The optimistic locking token.</param>
        /// <returns>The optimistic locking field's value.</returns>
        public static DateTime TokenToDateTime([NotNull] string token) 
            => new DateTime(Convert.ToInt64(token, 16) * IgnoredTicks);
    }
}