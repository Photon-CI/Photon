using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Artifacts
{
    [Serializable]
    public class ArtifactManagerClient
    {
        private readonly ArtifactManagerBoundary artifactMgr;


        public ArtifactManagerClient(ArtifactManagerBoundary artifactMgr)
        {
            this.artifactMgr = artifactMgr;
        }

        public async Task<string> GetBuildArtifact(string projectId, uint buildNumber, string filename)
        {
            return await RemoteTaskCompletionSource<string>.Run(task => {
                artifactMgr.GetBuildArtifact(projectId, buildNumber, filename, task);
            });
        }

        public async Task<string> GetDeploymentArtifact(string projectId, uint deploymentNumber, string filename)
        {
            return await RemoteTaskCompletionSource<string>.Run(task => {
                artifactMgr.GetDeploymentArtifact(projectId, deploymentNumber, filename, task);
            });
        }

        public async Task ArchiveArtifact(string filename)
        {
            await RemoteTaskCompletionSource.Run(task => {
                artifactMgr.ArchiveArtifact(filename, task);
            });
        }
    }
}
