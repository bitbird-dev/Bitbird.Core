using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Bitbird.Core.Benchmarks;

namespace Bitbird.Core.Data.Query
{
    /// <summary>
    /// Provides various methods to build queries based on other queries.
    /// </summary>
    public static class QueryBuilderSetup
    {
        static QueryBuilderSetup()
        {
            QueryBuilder.LikeMethod = typeof(DbFunctions).GetMethods().Single(method => method.Name == nameof(DbFunctions.Like) && method.GetParameters().Length == 2);
            QueryBuilder.LikeMethodInstance = null;
        }


        /// <summary>
        /// Execute the query.
        /// Will fail if called on in-memory-queries.
        /// </summary>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <param name="query">The query to execute. Must not be null.</param>
        /// <param name="benchmarks">A benchmark section. Can be null.</param>
        /// <returns>A query result.</returns>
        public static Task<QueryResult<TModel>> ExecuteAsync<TModel>(this BuiltQuery<TModel> query, BenchmarkSection benchmarks)
            where TModel : class
            => query.ExecuteAsync(benchmarks, x => x.ToArrayAsync(), x => x.LongCountAsync());
    }
}