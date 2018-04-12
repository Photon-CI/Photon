using System;

namespace Photon.Framework.Agent
{
    [Serializable]
    public class AgentBuildContext : AgentContextBase, IAgentBuildContext
    {
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public int BuildNumber {get; set;}
    }
}
