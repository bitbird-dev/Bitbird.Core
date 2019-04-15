using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Bitbird.Core.Query
{
    public class QueryInfo<T> : QueryInfo
    {
        public QueryInfo(
            QuerySortProperty<T>[] sortProperties = null, 
            Expression<Func<T,bool>> filterExpression = null, 
            QueryPaging paging = null, 
            Expression<Func<T,object>>[] includes = null)
        : this(
            sortProperties,
            filterExpression == null ? null : QueryFilter.Parse(filterExpression),
            paging,
            includes == null ? null : includes.Select(include => EncodeMemberExpression((MemberExpression)include.Body, include.Parameters[0])).ToArray())
        {
        }
        private QueryInfo(
            QuerySortProperty<T>[] sortProperties = null,
            QueryFilter[] filters = null,
            QueryPaging paging = null,
            string[] includes = null)
            : base(
                sortProperties,
                filters,
                paging,
                includes)
        {
        }

        public QueryInfo<T> AddFilter(Expression<Func<T, bool>> filterExpression)
        {
            return new QueryInfo<T>(
                SortProperties?.Cast<QuerySortProperty<T>>().ToArray(), 
                (Filters ?? new QueryFilter[0]).Concat(QueryFilter.Parse(filterExpression)).ToArray(),
                Paging,
                Includes);
        }
        public QueryInfo<T> AddFilter(params QueryFilter[] filters)
        {
            return new QueryInfo<T>(
                SortProperties?.Cast<QuerySortProperty<T>>().ToArray(),
                (Filters ?? new QueryFilter[0]).Concat(filters).ToArray(),
                Paging,
                Includes);
        }
        public QueryInfo<T> AddIncludes(params Expression<Func<T, object>>[] includeExpression)
        {
            return new QueryInfo<T>(
                SortProperties?.Cast<QuerySortProperty<T>>().ToArray(),
                Filters,
                Paging,
                (Includes ?? new string[0]).Concat(includeExpression.Select(e => QueryInfo.EncodeMemberExpression((MemberExpression)e.Body, e.Parameters[0]))).ToArray());
        }
        public QueryInfo<T> AddPaging(int pageSize, int page)
        {
            return new QueryInfo<T>(
                SortProperties?.Cast<QuerySortProperty<T>>().ToArray(),
                Filters,
                new QueryPaging(pageSize, page),
                Includes);
        }
    }

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
        

        internal static string EncodeMemberExpression(MemberExpression memberExpression, ParameterExpression parameter)
        {
            string instance = null;
            switch (memberExpression.Expression)
            {
                case MemberExpression instanceMemberExpression:
                    instance = EncodeMemberExpression(instanceMemberExpression, parameter);
                    break;
                case ParameterExpression parameterExpression:
                    if (parameter != null && (parameterExpression.Name != parameter.Name || parameterExpression.Type != parameter.Type))
                        throw new Exception($"{nameof(EncodeMemberExpression)}: Unknown parameter found: {parameterExpression.Name} of Type {parameterExpression.Type}. The only known parameter is {parameter.Name} of Type {parameter.Type}.");
                    break;
            }

            return instance != null
                ? $"{instance}.{memberExpression.Member.Name}"
                : memberExpression.Member.Name;
        }
    }

    public static class QueryInfoExtensions
    {
        public static QueryInfo PopFilterIfExists(this QueryInfo queryInfo, string propertyExpression, out QueryFilter foundFilter)
        {
            if (queryInfo == null)
            {
                foundFilter = null;
                return queryInfo;
            }

            var foundFilters = queryInfo.Filters?
                .Where(f => string.Equals(f.PropertyName, propertyExpression,
                    StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            if (foundFilters == null || foundFilters.Length == 0)
            {
                foundFilter = null;
                return queryInfo;
            }

            if (foundFilters.Length > 1)
                throw new Exception($"{nameof(QueryInfo)}: Two filters for the same property have been found. Property: {propertyExpression}. Found filters: {string.Join("|", foundFilters.AsEnumerable())}. Since this property is processed manually, only one filter is allowed.");

            foundFilter = foundFilters[0];
            return new QueryInfo(
                queryInfo.SortProperties, 
                queryInfo.Filters.Except(foundFilters).ToArray(),
                queryInfo.Paging,
                queryInfo.Includes);
        }
    }
}