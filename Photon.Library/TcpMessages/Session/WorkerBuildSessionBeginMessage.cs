using Photon.Communication.Messages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Variables;

namespace Photon.Library.TcpMessages.Session
{
    public class WorkerBuildSessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public string AgentSessionId {get; set;}
        public string SessionClientId {get; set;}
        public string AssemblyFilename {get; set;}
        public string WorkDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public string BinDirectory {get; set;}
        public Project Project {get; set;}
        public ServerAgent Agent {get; set;}
        public VariableSetCollection ServerVariables {get; set;}
        public VariableSetCollection AgentVariables {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public uint BuildNumber {get; set;}
        public string CommitHash {get; set;}
        public string CommitAuthor {get; set;}
        public string CommitMessage {get; set;}
    }

    //public class WorkerBuildSessionBeginResponse : ResponseMessageBase
    //{
    //    public string ServerSessionId {get; set;}
    //}
}
