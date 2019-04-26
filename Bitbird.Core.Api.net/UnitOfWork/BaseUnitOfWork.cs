using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BackRohr.Api.Models.Core;
using BackRohr.Api.Service.Core;
using Bitbird.Core.Data.Net.Cache;
using JetBrains.Annotations;

namespace BackRohr.Api.Service
{
    public class BaseUnitOfWork : IDisposable
    {
        private bool isDisposed;
        [NotNull, ItemNotNull]
        private readonly List<EntityChangeModel> entityChangeNotifications = new List<EntityChangeModel>();
        [NotNull]
        private readonly DbContextTransaction transaction;
        [CanBeNull]
        private readonly DeferredRedisOperations redisDeferred;

        protected BaseUnitOfWork([NotNull] DbContext db, [CanBeNull] Redis redis)
        {
            transaction = db.Database.BeginTransaction();
            redisDeferred = redis?.StartDeferred();
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
            if (redisDeferred == null)
                throw new Exception("Redis was not configured.");

            transaction.Commit();

            var redisTask = redisDeferred.ExecuteAsync();

            HandleEntityChangeNotifications();

            await redisTask;
        }

        protected virtual void HandleEntityChangeNotifications()
        {
        }


        public void PushEntityChangeNotifications([NotNull, ItemNotNull] IEnumerable<EntityChangeModel> newChanges)
        {
            lock (entityChangeNotifications)
            {
                entityChangeNotifications.AddRange(newChanges);
            }
        }
        public void PushEntityChangeNotifications(EntityType entityType, [NotNull] IEnumerable<long> ids, EntityChangeType changeType)
        {
            PushEntityChangeNotifications(ids.Select(id => new EntityChangeModel
            {
                Id = id,
                Entity = entityType,
                ChangeType = changeType
            }).ToArray());
        }

        [NotNull, ItemNotNull]
        protected EntityChangeModel[] PopEntityChangeNotifications()
        {
            EntityChangeModel[] data;
            lock (entityChangeNotifications)
            {
                data = entityChangeNotifications.ToArray();
                entityChangeNotifications.Clear();
            }

            return EntityChangeModel.Accumulate(data);
        }
    }
}