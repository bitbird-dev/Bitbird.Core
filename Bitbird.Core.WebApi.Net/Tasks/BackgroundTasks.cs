using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Hosting;

namespace Bitbird.Core.WebApi.Tasks
{
    /// <summary>
    /// Provides helper methods to run/schedule background tasks.
    /// Is a singleton (see <see cref="BackgroundTasks.Instance"/>).
    /// </summary>
    public class BackgroundTasks
    {
        /// <summary>
        /// Schedules a task to run in the background.
        /// </summary>
        /// <param name="backgroundTask">The task to run.</param>
        /// <param name="name">A name for the task. Can be null.</param>
        /// <param name="delay">The initial delay for the task.
        /// After this timespan the task is executed the first time after scheduling.
        ///
        /// If this is null, the task is executed immediately.</param>
        /// <param name="interval">The interval after which, after the last execution, the task is run again.
        ///
        /// If this is null, the task is not re-scheduled after execution, but only run once.</param>
        public void Schedule(IBackgroundTask backgroundTask, string name = null, TimeSpan? delay = null, TimeSpan? interval = null)
        {
            if (backgroundTask == null)
                throw new ArgumentNullException(nameof(backgroundTask), @"Cannot schedule a the given task since it is null.");

            if (!delay.HasValue)
                Run(backgroundTask);

            void CacheCallback(string key, object value, CacheItemRemovedReason reason)
            {
                if (delay.HasValue)
                    Run(backgroundTask);

                if (interval.HasValue)
                    Schedule(backgroundTask, name, interval, interval);
            }

            HostingEnvironment.Cache.Insert($"BgTask.{name ?? "Unnamed"}", 
                new
                {
                    Name = name,
                    Delay = delay,
                    Interval = interval,
                    Task = backgroundTask
                }, 
                null,
                DateTime.Now + (delay ?? TimeSpan.Zero), 
                Cache.NoSlidingExpiration, 
                CacheItemPriority.NotRemovable, 
                CacheCallback);
        }

        /// <summary>
        /// Immediately runs a background task.
        /// </summary>
        /// <param name="backgroundTask">The task to run.</param>
        public void Run(IBackgroundTask backgroundTask)
        {
            async Task TaskCallback(CancellationToken cancellationToken)
            {
                try
                {
                    await backgroundTask.Run(cancellationToken);
                }
                catch
                {
                    // TODO: Log details
                }
            }

            HostingEnvironment.QueueBackgroundWorkItem(TaskCallback);
        }

        private BackgroundTasks() { }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static BackgroundTasks Instance = new BackgroundTasks();
    }
}