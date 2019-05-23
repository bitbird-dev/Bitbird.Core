using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public class Validator : ValidatorBase
    {
        protected override Task<bool> AnyAsync<T>(IQueryable<T> query)
        {
            return query.AnyAsync();
        }
    }
}
