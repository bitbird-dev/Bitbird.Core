namespace Bitbird.Core.Query
{
    public class QueryFreeTextFilter : QueryFilter
    {
        public readonly string Pattern;

        public QueryFreeTextFilter(string propertyName, string pattern) : base(propertyName)
        {
            Pattern = pattern;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Pattern)}: {Pattern}";
        }
    }
}