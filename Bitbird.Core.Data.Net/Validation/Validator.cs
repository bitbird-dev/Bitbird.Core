using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Validation
{
    public class Validator : ValidatorBase
    {
        protected override async Task<bool> AnyAsync<T>(IQueryable<T> query)
        {
            return query.IsInMemory() 
                ? query.Any() 
                : await query.AnyAsync();
        }
    }
}
