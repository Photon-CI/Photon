using Photon.Communication.Messages;
using Photon.Framework.Projects;
using Photon.Framework.Variables;

namespace Photon.Library.Communication.Messages.Session
{
    public class AgentBuildSessionRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}

        public string ServerSessionId {get; set;}
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        //public string WorkDirectory {get; set;}
        //public string BinDirectory {get; set;}
        //public string ContentDirectory {get; set;}
        public VariableSetCollection ServerVariables {get; set;}

        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public uint BuildNumber {get; set;}
        public string PreBuildCommand {get; set;}
        public string CommitHash {get; set;}
        //public string CommitAuthor {get; set;}
        //public string CommitMessage {get; set;}
    }

    public class AgentBuildSessionRunResponse : ResponseMessageBase
    {
        //
    }
}
