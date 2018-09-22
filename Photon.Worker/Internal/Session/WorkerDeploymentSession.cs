using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Session
{
    internal class WorkerDeploymentSession : WorkerSession
    {
        public override Task Initialize(CancellationToken token = default)
        {
            return base.Initialize(token);

            LoadProjectAssembly();
        }

        public override Task Run(CancellationToken token = default)
        {
            return base.Run(token);
        }

        public override Task Release(CancellationToken token = default)
        {
            return base.Release(token);
        }
    }
}
