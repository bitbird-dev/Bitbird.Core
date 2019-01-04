namespace Bitbird.Core.Query
{
    public class QueryInfo
    {
        public readonly QuerySortProperty[] SortProperties;
        public readonly QueryFilter[] Filters;
        public readonly QueryPaging Paging;
        public readonly string[] Includes;

        public QueryInfo() : this(null, null, null, null)
        {
        }
        public QueryInfo(QuerySortProperty[] sortProperties, QueryFilter[] filters, QueryPaging paging, string[] includes)
        {
            SortProperties = sortProperties;
            Filters = filters;
            Paging = paging;
            Includes = includes;
        }
    }
}