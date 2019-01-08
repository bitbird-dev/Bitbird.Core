using System;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.DbContext
{
    public static class DbSetExtensions
    {
        private static readonly Regex Regex = new Regex(@"FROM\s+(?<table>.+)\s+AS", RegexOptions.Compiled);

        public static string GetTableName<T>(this DbSet<T> set) where T : class
        {
            var match = Regex.Match(set.Sql);

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
