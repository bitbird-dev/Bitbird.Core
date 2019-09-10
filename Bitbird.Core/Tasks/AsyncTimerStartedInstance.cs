using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Tasks
{
    internal class AsyncTimerStartedInstance : IDisposable
    {
        [NotNull] private readonly CancellationTokenSource ownCancellationTokenSource;
        [NotNull] private readonly CancellationTokenSource linkedCancellationTokenSource;
        [NotNull] private readonly TaskCompletionSource<object> cancellationCompleteSource;

        public AsyncTimerStartedInstance(CancellationToken cancellationToken)
        {
            try
            {
                cancellationCompleteSource = new TaskCompletionSource<object>();
                ownCancellationTokenSource = new CancellationTokenSource();
                linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ownCancellationTokenSource.Token, cancellationToken);
            }
            catch
            {
                try { Dispose(); } catch { /* ignored */ }
                throw;
            }
        }

        public CancellationToken CancellationToken => linkedCancellationTokenSource.Token;

        public void Cancel() => ownCancellationTokenSource.Cancel();

        [NotNull]
        public Task WaitForEndAsync() => cancellationCompleteSource.Task;
        
        public void Dispose()
        {
            try { cancellationCompleteSource.SetResult(null); } catch { /* ignored */ }
            try { linkedCancellationTokenSource.Dispose(); } catch { /* ignored */ }
            try { ownCancellationTokenSource.Dispose(); } catch { /* ignored */ }
        }
    }
}