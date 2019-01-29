using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bitbird.Core.Tasks
{
    /// <summary>
    /// See <see href="https://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously"/>.
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// Executes an async <see cref="Task{TResult}"/> method which has a void return value synchronously.
        /// </summary>
        /// <param name="task"><see cref="Task{TResult}"/> method to execute.</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Executes an async <see cref="Task{TResult}"/> method which has a T return type synchronously.
        /// </summary>
        /// <typeparam name="T">Return type of the task and therefore this method.</typeparam>
        /// <param name="task"><see cref="Task{TResult}"/> method to execute.</param>
        /// <returns>The value returned by the task.</returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default;
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }
    }
}
