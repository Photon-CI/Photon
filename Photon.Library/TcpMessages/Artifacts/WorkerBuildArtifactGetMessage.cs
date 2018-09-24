using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Artifacts
{
    public class WorkerBuildArtifactGetRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ProjectId {get; set;}
        public uint BuildNumber {get; set;}
        public string Filename {get; set;}
    }

    public class WorkerBuildArtifactGetResponse : ResponseMessageBase, IFileResponseMessage
    {
        public string Filename {get; set;}
    }
}
