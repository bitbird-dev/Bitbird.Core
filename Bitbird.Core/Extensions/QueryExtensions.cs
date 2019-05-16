using System;
using System.Collections;
using System.Linq;

namespace Bitbird.Core
{
    public static class QueryExtensions
    {
        public static bool IsInMemory(this IQueryable query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var providerType = query.Provider.GetType();
            return typeof(IEnumerable).IsAssignableFrom(providerType) || providerType.IsArray;
        }
    }
}