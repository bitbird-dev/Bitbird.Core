using System;
using System.Threading.Tasks;
#if NET_CORE
using Microsoft.EntityFrameworkCore;
#else
using System.Data.Entity;

namespace Bitbird.Core.Data.DbContext
{
    public static class DbSetExtensions
    {
        public static string GetTableName<T>(this DbSet<T> set) where T : class
        {
            var match = BaseDbSetExtensions.Regex.Match(set.Sql);

            if (!match.Success)
                throw new Exception("Failed to parse table sql.");

            return match.Groups["table"].Value;
        }
        public static async Task DeleteAllAsync<T>(this DbSet<T> set, System.Data.Entity.DbContext db) where T : class
        {
            await db.Database.ExecuteSqlCommandAsync($"DELETE FROM {set.GetTableName()}");
        }
    }
}
#endif

// TODO
/*
namespace Bitbird.Core.Data.DbContext
{
    public static class DbSetExtensions
    {
        public static string GetTableName<T>(this DbSet<T> set) where T : class
        {
            var match = BaseDbSetExtensions.Regex.Match(set.Sql);

            if (!match.Success)
                throw new Exception("Failed to parse table sql.");

            return match.Groups["table"].Value;
        }
        public static async Task DeleteAllAsync<T>(this DbSet<T> set, System.Data.Entity.DbContext db) where T : class
        {
            await db.Database.ExecuteSqlCommandAsync($"DELETE FROM {set.GetTableName()}");
        }
    }
}
*/