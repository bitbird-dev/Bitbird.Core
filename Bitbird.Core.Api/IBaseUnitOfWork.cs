using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitbird.Core.Api.EntityChanges;
using Bitbird.Core.Data.Cache;
using JetBrains.Annotations;

namespace Bitbird.Core.Api
{
    public interface IBaseUnitOfWork<in TEntityChangeModel, in TEntityTypeId, in TId> : IDisposable
        where TEntityChangeModel : class
    {
        DeferredRedisOperations RedisDeferred { get; }

        Task SubmitAsync();

        void PushEntityChangeNotifications([NotNull, ItemNotNull] IEnumerable<TEntityChangeModel> newChanges);

        void PushEntityChangeNotifications(TEntityTypeId entityType, [NotNull] IEnumerable<TId> ids, EntityChangeType changeType);
    }
}