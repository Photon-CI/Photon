using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Scripts
{
    public class ScriptAgentCollection
    {
        private readonly ScriptAgent[] scriptAgents;
        private ScriptAgentSession[] sessions;


        public ScriptAgentCollection(IEnumerable<ScriptAgent> scriptAgents)
        {
            this.scriptAgents = scriptAgents as ScriptAgent[] ?? scriptAgents.ToArray();
        }

        public void Initialize()
        {
            sessions = scriptAgents
                .Select(x => x.BeginSession())
                .ToArray();
        }

        public void Release()
        {
            foreach (var session in sessions)
                session.Release();
        }

        public void RunTasks(params string[] taskNames)
        {
            //...
        }
    }
}
