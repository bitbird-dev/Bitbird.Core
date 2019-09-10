using System.Threading;
using System.Threading.Tasks;

namespace Bitbird.Core.Tasks
{
    public delegate Task AsyncTimerActionDelegate(CancellationToken cancellationToken);
}