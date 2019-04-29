using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Bitbird.Core.Api.Net.EntityChanges;
using Bitbird.Core.Data.Net.Cache;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Net
{
    public abstract class BaseUnitOfWork<TEntityChangeModel, TEntityTypeId, TId> : IDisposable
        where TEntityChangeModel : class
    {
        private bool isDisposed;
        [NotNull, ItemNotNull]
        private readonly List<TEntityChangeModel> entityChangeNotifications = new List<TEntityChangeModel>();
        [NotNull]
        private readonly DbContextTransaction transaction;
        [CanBeNull]
        public DeferredRedisOperations RedisDeferred { get; }

        protected BaseUnitOfWork([NotNull] DbContext db, [CanBeNull] Redis redis)
        {
            transaction = db.Database.BeginTransaction();
            RedisDeferred = redis?.StartDeferred();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (isDisposed)
                    return;

                isDisposed = true;
            }

            transaction.Dispose();
        }

        public async Task SubmitAsync()
        {
            if (RedisDeferred == null)
                throw new Exception("Redis was not configured.");

            transaction.Commit();

            var redisTask = RedisDeferred.ExecuteAsync();

            HandleEntityChangeNotifications();

            await redisTask;
        }

        protected virtual void HandleEntityChangeNotifications()
        {
        }


        public void PushEntityChangeNotifications([NotNull, ItemNotNull] IEnumerable<TEntityChangeModel> newChanges)
        {
            lock (entityChangeNotifications)
            {
                entityChangeNotifications.AddRange(newChanges);
            }
        }
        public void PushEntityChangeNotifications(TEntityTypeId entityType, [NotNull] IEnumerable<TId> ids, EntityChangeType changeType)
        {
            PushEntityChangeNotifications(ids.Select(id => CreateEntityChangeModel(id, entityType, changeType)).ToArray());
        }

        [NotNull, ItemNotNull]
        protected TEntityChangeModel[] PopEntityChangeNotifications()
        {
            TEntityChangeModel[] data;
            lock (entityChangeNotifications)
            {
                data = entityChangeNotifications.ToArray();
                entityChangeNotifications.Clear();
            }

            return AccumulateEntityChangeModels(data);
        }


        [NotNull]
        protected virtual TEntityChangeModel[] AccumulateEntityChangeModels([NotNull, ItemNotNull] TEntityChangeModel[] models) => models;
        [NotNull]
        protected abstract TEntityChangeModel CreateEntityChangeModel([NotNull] TId id, TEntityTypeId entityType, EntityChangeType changeType);
    }
}