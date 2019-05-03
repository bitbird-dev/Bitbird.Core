using System.Linq;

namespace Bitbird.Core.Data.Query
{ 
    public class BuiltQuery<TModel>
        where TModel : class
    {
        public readonly IQueryable<TModel> Query;
        public readonly IQueryable<TModel> PagedQuery;
        public readonly long PageSize;

        public BuiltQuery(IQueryable<TModel> query, IQueryable<TModel> pagedQuery, long pageSize)
        {
            Query = query;
            PagedQuery = pagedQuery;
            PageSize = pageSize;
        }
    }
}