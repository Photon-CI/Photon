using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Artifacts
{
    public interface IArtifactDeploymentClient : IArtifactClient
    {
        Task ArchiveDeploymentArtifactAsync(uint deploymentNumber, string filename, CancellationToken token = default);
    }
}
