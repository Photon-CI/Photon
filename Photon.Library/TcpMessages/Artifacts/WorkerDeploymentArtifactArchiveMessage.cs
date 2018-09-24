using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Artifacts
{
    public class WorkerDeploymentArtifactArchiveRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}
        public string ProjectId {get; set;}
        public uint DeploymentNumber {get; set;}
        public string Filename {get; set;}
    }

    //public class WorkerDeploymentArtifactArchiveResponse : ResponseMessageBase
    //{
    //}
}
