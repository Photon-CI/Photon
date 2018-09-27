using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Artifacts
{
    public class WorkerDeploymentArtifactGetRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ProjectId {get; set;}
        public uint DeploymentNumber {get; set;}
        public string Filename {get; set;}
    }

    public class WorkerDeploymentArtifactGetResponse : ResponseMessageBase, IFileResponseMessage
    {
        public string Filename {get; set;}
    }
}
