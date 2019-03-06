using System;
using System.Text;

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
        public QueryInfo(QuerySortProperty[] sortProperties = null, QueryFilter[] filters = null, QueryPaging paging = null, string[] includes = null)
        {
            SortProperties = sortProperties;
            Filters = filters;
            Paging = paging;
            Includes = includes;
        }

        public string ToQueryParameterString()
        {
            var sb = new StringBuilder();

            var dataWritten = false;

            if ((SortProperties?.Length ?? 0) != 0)
            {
                dataWritten = true;

                sb.Append("sort=");
                var first = true;
                foreach (var sortProperty in SortProperties)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(',');

                    if (!sortProperty.Ascending)
                        sb.Append('-');
                    
                    sb.Append(Uri.EscapeUriString(sortProperty.PropertyName));
                }
            }

            if ((Filters?.Length ?? 0) != 0)
            {
                foreach (var filter in Filters)
                {
                    if (dataWritten)
                        sb.Append('&');
                    else
                        dataWritten = true;

                    sb.AppendFormat("filter[{0}]={1}", Uri.EscapeUriString(filter.PropertyName), Uri.EscapeUriString(filter.ValueExpression));
                }
            }

            if (Paging != null)
            {
                if (dataWritten)
                    sb.Append('&');
                else
                    dataWritten = true;

                sb.AppendFormat("page[size]={0}&page[number]={1}", Paging.PageSize, Paging.Page);
            }

            if ((Includes?.Length ?? 0) != 0)
            {
                if (dataWritten)
                    sb.Append('&');

                sb.Append("include=");
                var first = true;
                foreach (var include in Includes)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(',');

                    sb.Append(Uri.EscapeUriString(include));
                }
            }

            return sb.ToString();
        }
    }
}