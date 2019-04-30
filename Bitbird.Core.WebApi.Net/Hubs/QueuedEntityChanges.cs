using System.Collections.Generic;

namespace Bitbird.Core.WebApi.Net.Hubs
{
    /// <summary>
    /// Stores queued entity changesModel.
    /// After construction this class is completely thread-safe.
    /// Enqueuing as well as dequeuing can be done concurrently.
    ///
    /// Enqueued models are copied only by reference.
    /// </summary>
    public class QueuedChanges<TChangeModel>
        where TChangeModel : class
    {
        private List<TChangeModel> queue = new List<TChangeModel>();

        /// <summary>
        /// Enqueues the entity changes.
        /// The passed collection is not referenced after the function executed, but all entities in this array are only copied by reference.
        /// Thread-safe.
        /// </summary>
        /// <param name="changesModel">A collection of changes to enqueue.</param>
        public void ConcurrentEnqueue(IEnumerable<TChangeModel> changesModel)
        {
            lock (this)
            {
                queue.AddRange(changesModel);
            }
        }

        /// <summary>
        /// Returns all currently queued elements and deletes them from the queue.
        /// Thread-safe.
        /// </summary>
        /// <returns>All currently queued elements.</returns>
        public List<TChangeModel> ConcurrentDequeueAll()
        {
            List<TChangeModel> values;

            lock (this)
            {
                values = queue;
                queue = new List<TChangeModel>();
            }

            return values;
        }
    }
}