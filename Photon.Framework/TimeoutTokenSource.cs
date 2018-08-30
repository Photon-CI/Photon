using System;
using System.Threading;

namespace Photon.Framework
{
    public class TimeoutTokenSource : IDisposable
    {
        private readonly CancellationToken parentToken;
        private readonly CancellationTokenSource timeoutTokenSource;
        private readonly CancellationTokenSource mergedTokenSource;

        public CancellationToken Token => mergedTokenSource?.Token ?? parentToken;


        public TimeoutTokenSource(int timeoutMs = 0, CancellationToken parentToken = default(CancellationToken))
        {
            this.parentToken = parentToken;

            if (timeoutMs > 0) {
                timeoutTokenSource = new CancellationTokenSource(timeoutMs);
                mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, parentToken);
            }
        }

        public void Dispose()
        {
            timeoutTokenSource?.Dispose();
            mergedTokenSource?.Dispose();
        }
    }
}
