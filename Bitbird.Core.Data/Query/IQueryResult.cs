using System.Collections;

namespace Bitbird.Core.Data.Query
{
    public interface IQueryResult
    {
        long RecordCount { get; }
        long PageCount { get; }
        IEnumerable Data { get; }
    }
}