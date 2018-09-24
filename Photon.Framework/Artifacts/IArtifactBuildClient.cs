using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Artifacts
{
    public interface IArtifactBuildClient : IArtifactClient
    {
        Task ArchiveBuildArtifactAsync(uint buildNumber, string filename, CancellationToken token = default);
    }
}
