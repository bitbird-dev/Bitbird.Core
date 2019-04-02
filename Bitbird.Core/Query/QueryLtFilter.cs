namespace Bitbird.Core.Query
{
    public class QueryLtFilter : QueryFilter
    {
        public readonly string Upper;

        public QueryLtFilter(string propertyName, string upper) : base(propertyName)
        {
            Upper = upper;
        }

        public override string ToString()
        {
            return $"{PropertyName} < {Upper}";
        }

        public override string ValueExpression => $"LT({Upper})";
    }
}