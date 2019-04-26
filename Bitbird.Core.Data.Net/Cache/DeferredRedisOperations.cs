using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbird.Core.Data.Net.Cache
{
    public class DeferredRedisOperations
    {
        private readonly Redis redis;
        private readonly List<DeferredRedisDeleteOperation> deleted = new List<DeferredRedisDeleteOperation>();

        internal DeferredRedisOperations(Redis redis)
        {
            this.redis = redis;
        }

        internal void PushOperation(DeferredRedisDeleteOperation operation)
        {
            lock (deleted)
            {
                deleted.Add(operation);
            }
        }

        internal DeferredRedisDeleteOperation PopDeleteOperations()
        {
            DeferredRedisDeleteOperation[] data;
            lock (deleted)
            {
                data = deleted.ToArray();
                deleted.Clear();
            }

            return new DeferredRedisDeleteOperation(
                data.SelectMany(x => x.Keys)
                    .Distinct()
                    .ToArray());
        }

        public async Task ExecuteAsync()
        {
            var delete = PopDeleteOperations();
            await redis.DeleteManyAsync(delete.Keys);
        }
    }
}