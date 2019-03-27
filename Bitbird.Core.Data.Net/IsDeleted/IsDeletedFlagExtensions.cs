using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbird.Core.Data.Net
{
    public static class IsDeletedFlagExtensions
    {
        public static bool HasDeletedState(this IIsDeletedFlagEntity entity, DeletedState state)
        {
            switch (state)
            {
                case DeletedState.NotDeleted:
                    return !entity.IsDeleted;
                case DeletedState.Deleted:
                    return entity.IsDeleted;
                case DeletedState.Any:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IQueryable<T> OfDeletedState<T>(this IQueryable<T> query, DeletedState state)
            where T : class, IIsDeletedFlagEntity
        {
            switch (state)
            {
                case DeletedState.NotDeleted:
                    return query.OfNotDeleted();
                case DeletedState.Deleted:
                    return query.OfDeleted();
                case DeletedState.Any:
                    return query;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }
        public static IEnumerable<T> OfDeletedState<T>(this IEnumerable<T> query, DeletedState state)
            where T : class, IIsDeletedFlagEntity
        {
            switch (state)
            {
                case DeletedState.NotDeleted:
                    return query.OfNotDeleted();
                case DeletedState.Deleted:
                    return query.OfDeleted();
                case DeletedState.Any:
                    return query;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private static IQueryable<T> OfDeleted<T>(this IQueryable<T> query) where T : class, IIsDeletedFlagEntity
        {
            return query.Where(_ => _.IsDeleted);
        }

        private static IQueryable<T> OfNotDeleted<T>(this IQueryable<T> query) where T : class, IIsDeletedFlagEntity
        {
            return query.Where(_ => _.IsDeleted == false);
        }

        private static IEnumerable<T> OfDeleted<T>(this IEnumerable<T> query) where T : class, IIsDeletedFlagEntity
        {
            return query.Where(_ => _.IsDeleted);
        }

        private static IEnumerable<T> OfNotDeleted<T>(this IEnumerable<T> query) where T : class, IIsDeletedFlagEntity
        {
            return query.Where(_ => _.IsDeleted == false);
        }
    }
}
