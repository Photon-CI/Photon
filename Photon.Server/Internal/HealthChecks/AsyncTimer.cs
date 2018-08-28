using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.HealthChecks
{
    internal delegate Task AsyncTimerElapsedEventHandler(CancellationToken cancellationToken);

    internal class AsyncTimer
    {
        public event AsyncTimerElapsedEventHandler Elapsed;

        private CancellationTokenSource tokenSource;

        public TimeSpan Interval {get; set;}


        public void Start()
        {
            tokenSource = new CancellationTokenSource();
            _ = Run(tokenSource.Token);
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        private async Task Run(CancellationToken cancellationToken)
        {
            await Task.Run(async () => {
                while (!cancellationToken.IsCancellationRequested) {
                    if (Elapsed != null)
                        await Elapsed(cancellationToken);

                    await Task.Delay(Interval, cancellationToken);
                }
            }, cancellationToken);
        }
    }
}
