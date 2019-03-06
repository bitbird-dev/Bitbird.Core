﻿namespace Bitbird.Core.Query
{
    public class QueryRangeFilter : QueryFilter
    {
        public readonly string Lower;
        public readonly string Upper;

        public QueryRangeFilter(string propertyName, string lower, string upper) : base(propertyName)
        {
            Lower = lower;
            Upper = upper;
        }

        public override string ToString()
        {
            return $"{nameof(Lower)}: {Lower}, {nameof(Upper)}: {Upper}";
        }

        public override string ValueExpression => $"RANGE({Lower};{Upper})";
    }
}