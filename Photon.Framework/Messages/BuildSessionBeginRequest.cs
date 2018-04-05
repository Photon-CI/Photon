using Photon.Communication;
using Photon.Framework.Projects;

namespace Photon.Framework.Messages
{
    public class BuildSessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public Project Project {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public string AssemblyFile {get; set;}
    }
}
