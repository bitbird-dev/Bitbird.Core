using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Cache
{
    public static class CacheHelper
    {
        /// <summary>
        /// The default lifetime of a cache entry.
        /// </summary>
        private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(356);

        /// <summary>
        /// Returns null if expire is false, otherwise a default cache lifetime is returned.
        /// </summary>
        /// <param name="expire">Whether the returned timespan expires the cache or not.</param>
        /// <returns></returns>
        [CanBeNull, ContractAnnotation("expire:true => notnull; expire:false => null")]
        public static TimeSpan? GetExpiration(bool expire)
        {
            return expire ? CacheTtl : (TimeSpan?)null;
        }
    }
}