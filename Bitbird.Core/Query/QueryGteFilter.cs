namespace Bitbird.Core.Query
{
    public class QueryGteFilter : QueryFilter
    {
        public readonly string Lower;

        public QueryGteFilter(string propertyName, string lower) : base(propertyName)
        {
            Lower = lower;
        }

        public override string ToString()
        {
            return $"{PropertyName} >= {Lower}";
        }

        public override string ValueExpression => $"GTE({Lower})";
    }
}