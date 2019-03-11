namespace Bitbird.Core.Query
{
    public class QueryGtFilter : QueryFilter
    {
        public readonly string Lower;

        public QueryGtFilter(string propertyName, string lower) : base(propertyName)
        {
            Lower = lower;
        }

        public override string ToString()
        {
            return $"{nameof(Lower)}: {Lower}";
        }

        public override string ValueExpression => $"GT({Lower})";
    }
}