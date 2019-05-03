using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbird.Core.Data
{
    public static class IsLockedFlagExtensions
    {
        public static bool HasLockedState(this IIsLockedFlagEntity entity, LockedState state)
        {
            switch (state)
            {
                case LockedState.NotLocked:
                    return !entity.IsLocked;
                case LockedState.Locked:
                    return entity.IsLocked;
                case LockedState.Any:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IQueryable<T> OfLockedState<T>(this IQueryable<T> query, LockedState state)
            where T : class, IIsLockedFlagEntity
        {
            switch (state)
            {
                case LockedState.NotLocked:
                    return query.OfInLocked();
                case LockedState.Locked:
                    return query.OfLocked();
                case LockedState.Any:
                    return query;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        public static IEnumerable<T> OfLockedState<T>(this IEnumerable<T> query, LockedState state)
            where T : class, IIsLockedFlagEntity
        {
            switch (state)
            {
                case LockedState.NotLocked:
                    return query.OfInLocked();
                case LockedState.Locked:
                    return query.OfLocked();
                case LockedState.Any:
                    return query;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private static IQueryable<T> OfLocked<T>(this IQueryable<T> query) where T : class, IIsLockedFlagEntity
        {
            return query.Where(_ => _.IsLocked);
        }

        private static IQueryable<T> OfInLocked<T>(this IQueryable<T> query) where T : class, IIsLockedFlagEntity
        {
            return query.Where(_ => !_.IsLocked);
        }

        public static IEnumerable<T> OfLocked<T>(this IEnumerable<T> query) where T : class, IIsLockedFlagEntity
        {
            return query.Where(_ => _.IsLocked);
        }

        public static IEnumerable<T> OfInLocked<T>(this IEnumerable<T> query) where T : class, IIsLockedFlagEntity
        {
            return query.Where(_ => !_.IsLocked);
        }
    }
}