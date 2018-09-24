using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Artifacts
{
    public interface IArtifactClient
    {
        Task<string> GetBuildArtifactAsync(uint buildNumber, string filename, CancellationToken token = default);
        Task<string> GetDeploymentArtifactAsync(uint deploymentNumber, string filename, CancellationToken token = default);
    }
}
