using System.Text.RegularExpressions;

namespace Bitbird.Core.Data.DbContext
{
    public static class BaseDbSetExtensions
    {
        public static readonly Regex Regex = new Regex(@"FROM\s+(?<table>.+)\s+AS", RegexOptions.Compiled);
    }
}
