using System.Threading;
using System.Threading.Tasks;

namespace Bitbird.Core.WebApi.Net.Tasks
{
    /// <summary>
    /// A task that can be run in the background or scheduled to run delayed or repeatedly.
    /// </summary>
    public interface IBackgroundTask
    {
        /// <summary>
        /// The function to execute when executing the task.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to indicate server shutdown or task abortion.</param>
        /// <returns>The task to run.</returns>
        Task Run(CancellationToken cancellationToken);
    }
}