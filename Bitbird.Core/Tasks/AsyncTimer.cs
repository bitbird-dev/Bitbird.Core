using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Tasks
{
    public class AsyncTimer : IDisposable
    {
        private readonly TimeSpan initialDelay;
        private readonly TimeSpan interval;
        private readonly bool autoReset;
        private readonly CancellationToken cancellationToken;
        [NotNull] private readonly SemaphoreSlim runningInstanceSemaphore;
        [CanBeNull] private AsyncTimerStartedInstance runningInstance;

        public event AsyncTimerActionDelegate Elapsed;
        public event AsyncTimerExceptionDelegate ExceptionOccurred;

        private AsyncTimer(
            TimeSpan initialDelay,
            TimeSpan interval, 
            bool autoReset,
            CancellationToken cancellationToken)
        {
            this.initialDelay = initialDelay;
            this.interval = interval;
            this.autoReset = autoReset;
            this.cancellationToken = cancellationToken;
            runningInstanceSemaphore = new SemaphoreSlim(1, 1);
        }

        [NotNull, ItemNotNull, UsedImplicitly]
        public static async Task<AsyncTimer> CreateAsync(
            TimeSpan initialDelay,
            TimeSpan interval,
            bool autoReset = true,
            bool enabled = true,
            CancellationToken? cancellationToken = null,
            [CanBeNull, ItemNotNull] params AsyncTimerActionDelegate[] elapsedActions)
        {
            var timer = new AsyncTimer(
                initialDelay,
                interval,
                autoReset,
                cancellationToken ?? CancellationToken.None);

            try
            {
                if (elapsedActions != null)
                    foreach (var elapsedAction in elapsedActions)
                        // ReSharper disable once ConstantNullCoalescingCondition
                        timer.Elapsed += elapsedAction ?? throw new ArgumentNullException(nameof(elapsedActions));

                if (enabled)
                {
                    await timer.StartAsync();
                }
            }
            catch
            {
                try { timer.Dispose(); } catch { /* ignored */ }
                throw;
            }

            return timer;
        }
        
        [UsedImplicitly]
        public bool IsEnabled
        {
            get
            {
                runningInstanceSemaphore.Wait();
                try
                {
                    return runningInstance != null;
                }
                finally
                {
                    runningInstanceSemaphore.Release();
                }
            }
        }

        [UsedImplicitly]
        public TimeSpan InitialDelay => initialDelay;

        [UsedImplicitly]
        public TimeSpan Interval => interval;

        [UsedImplicitly]
        public bool AutoReset => autoReset;

        [NotNull]
        public async Task StartAsync()
        {
            // ReSharper disable once MethodSupportsCancellation
            await runningInstanceSemaphore.WaitAsync();
            try
            {
                if (runningInstance != null)
                    return;

                runningInstance = new AsyncTimerStartedInstance(cancellationToken);
#pragma warning disable 4014 // Don't await, just schedule in background
                // ReSharper disable once MethodSupportsCancellation
                Task.Run(() => MainLoopAsync(runningInstance));
#pragma warning restore 4014
            }
            finally
            {
                runningInstanceSemaphore.Release();
            }
        }

        [NotNull]
        public async Task StopAsync()
        {
            AsyncTimerStartedInstance currentInstance;

            // ReSharper disable once MethodSupportsCancellation
            await runningInstanceSemaphore.WaitAsync();
            try
            {
                currentInstance = runningInstance;
                if (currentInstance == null)
                    return;

                currentInstance.Cancel();
            }
            finally
            {
                runningInstanceSemaphore.Release();
            }

            await currentInstance.WaitForEndAsync();
        }

        [NotNull]
        private async Task MainLoopAsync([NotNull] AsyncTimerStartedInstance currentInstance)
        {
            if (currentInstance == null) throw new ArgumentNullException(nameof(currentInstance));
            try
            {
                currentInstance.CancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(initialDelay, currentInstance.CancellationToken);

                do
                {
                    currentInstance.CancellationToken.ThrowIfCancellationRequested();

                    var actions = Elapsed
                        ?.GetInvocationList()
                        .ToArray()
                        .Cast<AsyncTimerActionDelegate>();

                    if (actions == null)
                        continue;

                    var cancellationTokenCopy = currentInstance.CancellationToken;
                    await Task.WhenAll(actions.Select(async action =>
                    {
                        try
                        {
                            await action(cancellationTokenCopy);
                        }
                        catch (Exception exception)
                        {
                            try
                            {
                                var task = ExceptionOccurred?.Invoke(action, exception);
                                if (task != null)
                                    await task;
                            }
                            catch { /* ignored */ }
                        }
                    }));

                    currentInstance.CancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(interval, currentInstance.CancellationToken);
                }
                while (autoReset);
            }
            catch (TaskCanceledException) { /* expected */ }
            finally
            {
                // ReSharper disable once MethodSupportsCancellation
                await runningInstanceSemaphore.WaitAsync();
                try
                {
                    if (runningInstance == currentInstance)
                        runningInstance = null;
                }
                finally
                {
                    runningInstanceSemaphore.Release();
                }

                try { currentInstance.Dispose(); } catch { /* ignored */ }
            }
        }

        public void Dispose()
        {
            try { AsyncHelper.RunSync(StopAsync); } catch { /* ignored */ }
            try { runningInstanceSemaphore.Dispose(); } catch { /* ignored */ }
        }
    }
}