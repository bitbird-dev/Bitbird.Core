using Bitbird.Core.Extensions;

namespace Bitbird.Core.Query
{
    public class QueryInFilter : QueryFilter
    {
        public readonly string[] Values;

        public QueryInFilter(string propertyName, string[] values) : base(propertyName)
        {
            Values = values;
        }

        public override string ToString()
        {
            return $"{PropertyName} IN ({Values.SequenceToString(", ")})";
        }

        public override string ValueExpression => $"IN({string.Join(";", Values)})";
    }
}