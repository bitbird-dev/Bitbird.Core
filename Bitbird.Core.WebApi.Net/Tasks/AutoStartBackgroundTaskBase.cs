using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bitbird.Core.WebApi.Tasks
{
    /// <summary>
    /// A background task that can automatically be scheduled at startup.
    /// </summary>
    public abstract class AutoStartBackgroundTaskBase : IBackgroundTask
    {
        /// <summary>
        /// The name of the background task.
        /// </summary>
        public virtual string Name => GetType().Name;
        /// <summary>The initial delay for the task.
        /// After this timespan the task is executed the first time after scheduling.
        ///
        /// If this is null, the task is executed immediately.
        /// </summary>
        public virtual TimeSpan? Delay => Interval;
        /// <summary>The interval after which, after the last execution, the task is run again.
        ///
        /// If this is null, the task is not re-scheduled after execution, but only run once.
        /// </summary>
        public abstract TimeSpan? Interval { get; }

        /// <inheritdoc />
        public abstract Task Run(CancellationToken cancellationToken);
    }
}