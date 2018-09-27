using Photon.Communication.Messages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Variables;

namespace Photon.Library.TcpMessages.Session
{
    public class WorkerDeploymentSessionBeginRequest : IRequestMessage
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
        public string TaskName {get; set;}
        public uint DeploymentNumber {get; set;}
        public string EnvironmentName {get; set;}
    }

    //public class WorkerDeploymentSessionBeginResponse : ResponseMessageBase
    //{
    //    public string ServerSessionId {get; set;}
    //}
}
