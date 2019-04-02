namespace Bitbird.Core.Query
{
    public class QueryLteFilter : QueryFilter
    {
        public readonly string Upper;

        public QueryLteFilter(string propertyName, string upper) : base(propertyName)
        {
            Upper = upper;
        }

        public override string ToString()
        {
            return $"{PropertyName} <= {Upper}";
        }

        public override string ValueExpression => $"LTE({Upper})";
    }
}