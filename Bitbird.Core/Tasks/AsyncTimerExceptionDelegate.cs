using System;
using System.Threading.Tasks;

namespace Bitbird.Core.Tasks
{
    public delegate Task AsyncTimerExceptionDelegate(AsyncTimerActionDelegate failedDelegate, Exception exception);
}