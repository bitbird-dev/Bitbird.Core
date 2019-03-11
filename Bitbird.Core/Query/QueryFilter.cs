namespace Bitbird.Core.Query
{
    public abstract class QueryFilter
    {
        public readonly string PropertyName;

        protected QueryFilter(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override string ToString()
        {
            return $"{nameof(PropertyName)}: {PropertyName}";
        }

        public abstract string ValueExpression { get; }

        public static QueryFilter Exact(string property, string value)
            => new QueryExactFilter(property, value);
        public static QueryFilter GreaterThan(string property, string lower)
            => new QueryGtFilter(property, lower);
        public static QueryFilter GreaterThanEqual(string property, string lower)
            => new QueryGteFilter(property, lower);
        public static QueryFilter LessThan(string property, string upper)
            => new QueryLtFilter(property, upper);
        public static QueryFilter LessThanEqual(string property, string upper)
            => new QueryLteFilter(property, upper);
        public static QueryFilter Range(string property, string lower, string upper)
            => new QueryRangeFilter(property, lower, upper);
        public static QueryFilter In(string property, string[] values)
            => new QueryInFilter(property, values);
        public static QueryFilter FreeText(string property, string pattern)
            => new QueryFreeTextFilter(property, pattern);
    }
}