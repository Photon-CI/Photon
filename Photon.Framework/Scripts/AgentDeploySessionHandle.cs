using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class AgentDeploySessionHandle : IAgentSessionHandle
    {
        private readonly ServerAgentDefinition definition;


        public AgentDeploySessionHandle(ServerAgentDefinition agentDefinition)
        {
            this.definition = agentDefinition;
        }

        public Task BeginAsync()
        {
            // TODO: Create and connect TCP message client

            throw new System.NotImplementedException();
        }

        public Task ReleaseAsync()
        {
            // TODO: Dispose TCP message client

            throw new System.NotImplementedException();
        }

        public Task RunTaskAsync(string taskName)
        {
            // TODO: 

            throw new System.NotImplementedException();
        }
    }
}
