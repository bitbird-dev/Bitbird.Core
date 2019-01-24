using System.Collections;

namespace Bitbird.Core.Data.Net.Query
{
    public class QueryResult<TModel> : IQueryResult
        where TModel : class
    {
        public TModel[] Data { get; }
        public long RecordCount { get; }
        public long PageCount { get; }

        IEnumerable IQueryResult.Data => Data;

        public QueryResult(TModel[] data, long recordCount, long pageCount)
        {
            Data = data;
            RecordCount = recordCount;
            PageCount = pageCount;
        }
    }
}