namespace Bitbird.Core.Query
{
    public class QuerySortProperty
    {
        public readonly string PropertyName;
        public readonly bool Ascending;

        public QuerySortProperty(string propertyName, bool ascending)
        {
            PropertyName = propertyName;
            Ascending = ascending;
        }
    }
}