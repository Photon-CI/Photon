using System.Threading.Tasks;
using Photon.Framework;

namespace Photon.Agent.Internal
{
    public class DeployScriptAgent
    {
        private readonly AgentDefinition definition;


        public DeployScriptAgent(AgentDefinition agentDefinition)
        {
            this.definition = agentDefinition;
        }

        public async Task<ScriptAgentSession> BeginSession()
        {
            var session = new ScriptAgentSession(definition);

            await session.BeginAsync();

            return session;
        }
    }
}
