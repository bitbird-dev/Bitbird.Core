using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbird.Core.Data
{
    public static class IsActiveFlagExtensions
    {
        public static bool HasActiveState(this IIsActiveFlagEntity entity, ActiveState state)
        {
            switch (state)
            {
                case ActiveState.Inactive:
                    return !entity.IsActive;
                case ActiveState.Active:
                    return entity.IsActive;
                case ActiveState.Any:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IQueryable<T> OfActiveState<T>(this IQueryable<T> query, ActiveState state)
            where T : class, IIsActiveFlagEntity
        {
            switch (state)
            {
                case ActiveState.Inactive:
                    return query.OfInactive();
                case ActiveState.Active:
                    return query.OfActive();
                case ActiveState.Any:
                    return query;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        public static IEnumerable<T> OfActiveState<T>(this IEnumerable<T> query, ActiveState state)
            where T : class, IIsActiveFlagEntity
        {
            switch (state)
            {
                case ActiveState.Inactive:
                    return query.OfInactive();
                case ActiveState.Active:
                    return query.OfActive();
                case ActiveState.Any:
                    return query;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private static IQueryable<T> OfActive<T>(this IQueryable<T> query) where T : class, IIsActiveFlagEntity
        {
            return query.Where(_ => _.IsActive);
        }

        private static IQueryable<T> OfInactive<T>(this IQueryable<T> query) where T : class, IIsActiveFlagEntity
        {
            return query.Where(_ => !_.IsActive);
        }

        public static IEnumerable<T> OfActive<T>(this IEnumerable<T> query) where T : class, IIsActiveFlagEntity
        {
            return query.Where(_ => _.IsActive);
        }

        public static IEnumerable<T> OfInactive<T>(this IEnumerable<T> query) where T : class, IIsActiveFlagEntity
        {
            return query.Where(_ => !_.IsActive);
        }
    }
}