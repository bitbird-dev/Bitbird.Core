using System.Linq;

namespace Bitbird.Core.Data.Validation
{
    public interface IGetQueryByEntity
    {
        IQueryable<TEntity> GetNonTrackingQuery<TEntity>() where TEntity : class;
        IQueryable<TEntity> GetTrackingQuery<TEntity>() where TEntity : class;
    }
}