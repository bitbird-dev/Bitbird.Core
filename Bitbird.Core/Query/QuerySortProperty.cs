using System;
using System.Linq.Expressions;

namespace Bitbird.Core.Query
{
    public class QuerySortProperty<T> : QuerySortProperty
    {
        public QuerySortProperty(Expression<Func<T, object>> propertyExpression, bool ascending = true) 
            : base(QueryInfo.EncodeMemberExpression(propertyExpression.Body, propertyExpression.Parameters[0]), ascending)
        {

        }
    }
    public class QuerySortProperty
    {
        public readonly string PropertyName;
        public readonly bool Ascending;

        public QuerySortProperty(string propertyName, bool ascending = true)
        {
            PropertyName = propertyName;
            Ascending = ascending;
        }
    }
}