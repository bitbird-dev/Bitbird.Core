using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bitbird.Core.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Tests.Forms.TestForms.AsyncTimer
{
    [TestForm]
    public partial class AsyncTimerTestForm : Form
    {
        private const int CountLines = 15;

        private int currentNumber;
        private Tasks.AsyncTimer timer;
        
        public AsyncTimerTestForm()
        {
            InitializeComponent();
        }

        private void Log([NotNull] string msg)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            BeginInvoke((Action)(() =>
            {
                lock (tbLog)
                {
                    var text = tbLog.Text + Environment.NewLine + msg;
                    var lines = text.Split(new [] { Environment.NewLine }, StringSplitOptions.None);
                    if (lines.Length > CountLines)
                        text = string.Join(Environment.NewLine, lines.Skip(lines.Length - CountLines));

                    tbLog.Text = text;
                    tbLog.SelectionLength = 0;
                    tbLog.SelectionStart = tbLog.Text.Length;
                }
            }));
        }

        private void SecureExecute([NotNull] Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            try
            {
                action();
            }
            catch (Exception exception)
            {
                Log($"Exception during call:{Environment.NewLine}{exception}");
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            SecureExecute(() =>
            {
                if (timer != null)
                    throw new Exception("Already created");

                var number = ++ currentNumber;
                timer = AsyncHelper.RunSync(() => Tasks.AsyncTimer.CreateAsync(
                    TimeSpan.FromSeconds(0.5), 
                    TimeSpan.FromSeconds(0.25),
                    true,
                    true,
                elapsedActions: new AsyncTimerActionDelegate [] { cancellationToken =>
                        {
                            Log($"Invoked {number}");
                            return Task.CompletedTask;
                        }
                    }));
                timer.ExceptionOccurred += (del, exception) =>
                {
                    Log($"Exception during invocation {number}{Environment.NewLine}{exception}");
                    return Task.CompletedTask;
                };
                Log("Created");
            });
        }

        private void btnStartAwait_Click(object sender, EventArgs e)
        {
            SecureExecute(() =>
            {
                AsyncHelper.RunSync(async () =>
                {
                    await timer.StartAsync();
                    Log("Started");
                });
            });
        }

        private void btnStopAwait_Click(object sender, EventArgs e)
        {
            SecureExecute(() =>
            {
                AsyncHelper.RunSync(async () =>
                {
                    await timer.StopAsync();
                    Log("Stopped");
                });
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SecureExecute(() =>
            {
                Task.Run(async () =>
                {
                    await timer.StopAsync();
                    Log("Stopped");
                });
            });
        }

        private void btnDispose_Click(object sender, EventArgs e)
        {
            SecureExecute(() =>
            {
                timer.Dispose();
                timer = null;
            });
        }

        private void timerInfo_Tick(object sender, EventArgs e)
        {
            lblInfo.Text = $"{(GC.GetTotalMemory(false) / 1024):##,###} KB";
        }

        private void timerGc_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }
    }
}
