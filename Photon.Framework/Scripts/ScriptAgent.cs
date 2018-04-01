using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class ScriptAgent
    {
        private readonly AgentDefinition definition;


        public ScriptAgent(AgentDefinition agentDefinition)
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
