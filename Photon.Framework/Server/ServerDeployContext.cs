using Photon.Communication;
using System;

namespace Photon.Framework.Server
{

    [Serializable]
    public class ServerDeployContext : ServerContextBase, IServerDeployContext
    {
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string ScriptName {get; set;}


        protected override IAgentSessionHandle CreateSessionHandle(ServerAgentDefinition agent, MessageProcessorRegistry registry)
        {
            return new AgentDeploySessionHandle(this, agent, registry);
        }
    }
}
