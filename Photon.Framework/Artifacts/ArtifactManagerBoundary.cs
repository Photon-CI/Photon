using Photon.Framework.Domain;

namespace Photon.Framework.Artifacts
{
    public delegate void GetBuildArtifactFunc(string projectId, uint buildNumber, string filename, RemoteTaskCompletionSource<string> taskHandle);
    public delegate void GetDeploymentArtifactFunc(string projectId, uint buildNumber, string filename, RemoteTaskCompletionSource<string> taskHandle);
    public delegate void ArchiveArtifactFunc(string filename, RemoteTaskCompletionSource taskHandle);

    public class ArtifactManagerBoundary : MarshalByRefInstance
    {
        public event GetBuildArtifactFunc OnGetBuildArtifact;
        public event GetDeploymentArtifactFunc OnGetDeploymentArtifact;
        public event ArchiveArtifactFunc OnArchiveArtifact;


        public void GetBuildArtifact(string projectId, uint buildNumber, string filename, RemoteTaskCompletionSource<string> taskHandle)
        {
            OnGetBuildArtifact?.Invoke(projectId, buildNumber, filename, taskHandle);
        }

        public void GetDeploymentArtifact(string projectId, uint deploymentNumber, string filename, RemoteTaskCompletionSource<string> taskHandle)
        {
            OnGetDeploymentArtifact?.Invoke(projectId, deploymentNumber, filename, taskHandle);
        }

        public void ArchiveArtifact(string filename, RemoteTaskCompletionSource taskHandle)
        {
            OnArchiveArtifact?.Invoke(filename, taskHandle);
        }
    }
}
