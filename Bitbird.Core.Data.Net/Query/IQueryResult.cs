using System.Collections;

namespace Bitbird.Core.Data.Net.Query
{
    public interface IQueryResult
    {
        long RecordCount { get; }
        long PageCount { get; }
        IEnumerable Data { get; }
    }
}