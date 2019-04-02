namespace Bitbird.Core.Query
{
    public class QueryExactFilter : QueryFilter
    {
        public readonly string Value;

        public QueryExactFilter(string propertyName, string value) : base(propertyName)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{PropertyName} == {Value}";
        }

        public override string ValueExpression => Value;
    }
}