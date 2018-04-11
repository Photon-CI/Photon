using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Server;
using Photon.Server.Internal.Sessions;
using System;

namespace Photon.Server.Internal
{
    [Serializable]
    public class ServerBuildContext : ServerContextBase, IServerBuildContext
    {
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public int BuildNumber {get; set;}


        protected override IAgentSessionHandle CreateSessionHandle(ServerAgentDefinition agent, MessageProcessorRegistry registry)
        {
            return new AgentBuildSessionHandle(this, agent, registry);
        }
    }
}
