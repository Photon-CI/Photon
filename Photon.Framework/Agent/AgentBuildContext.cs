using Photon.Framework.Applications;
using System;

namespace Photon.Framework.Agent
{
    [Serializable]
    public class AgentBuildContext : AgentContextBase, IAgentBuildContext
    {
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public uint BuildNumber {get; set;}
        public string CommitHash {get; set;}
        public string CommitAuthor {get; set;}
        public string CommitMessage {get; set;}
        public IApplicationReader Applications {get; set;}
    }
}
