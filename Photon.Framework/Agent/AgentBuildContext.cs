using Photon.Framework.Applications;

namespace Photon.Framework.Agent
{
    public class AgentBuildContext : AgentContextBase, IAgentBuildContext
    {
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public uint BuildNumber {get; set;}
        public string PreBuildCommand {get; set;}
        public string CommitHash {get; set;}
        public string CommitAuthor {get; set;}
        public string CommitMessage {get; set;}
        public IApplicationReader Applications {get; set;}
    }
}
