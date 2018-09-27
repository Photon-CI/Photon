using Photon.Communication.Messages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Variables;
using Photon.Library.GitHub;

namespace Photon.Library.TcpMessages
{
    public class BuildSessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public string SessionClientId {get; set;}
        public Project Project {get; set;}
        public ServerAgent Agent {get; set;}
        public string AssemblyFile {get; set;}
        public string PreBuild {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public uint BuildNumber {get; set;}
        public VariableSetCollection Variables {get; set;}
        public GithubCommit Commit {get; set;}
    }
}
