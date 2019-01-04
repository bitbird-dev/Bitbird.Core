using System;

namespace Bitbird.Core.Query
{
    public class QueryPaging
    {
        public readonly int PageSize;
        public readonly int Page;

        public QueryPaging(int pageSize, int page)
        {
            if (pageSize < 1) throw new ArgumentException($"Page size must be 1 or larger (Details: pageSize={pageSize}).", nameof(pageSize));
            if (page < 0) throw new ArgumentException($"Page must be 0 or larger (Details: pageSize={page}).", nameof(page));

            PageSize = pageSize;
            Page = page;
        }
    }
}