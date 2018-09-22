using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Session
{
    internal class WorkerTestSession : WorkerSession
    {
        public override async Task Initialize(CancellationToken token = default)
        {
            await base.Initialize(token);

            Output.WriteLine("[TestSession.Initialize]", ConsoleColor.White);
        }

        public override Task Run(CancellationToken token = default)
        {
            Output.WriteLine("[TestSession.Run]", ConsoleColor.White);

            return base.Run(token);
        }

        public override Task Release(CancellationToken token = default)
        {
            Output.WriteLine("[TestSession.Release]", ConsoleColor.White);

            return base.Release(token);
        }
    }
}
