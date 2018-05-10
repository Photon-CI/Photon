using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Photon.Framework.Server
{
    [Serializable]
    public abstract class ServerContextBase : DomainContextBase, IServerContext
    {
        public ServerAgent[] Agents {get; set;}
        public string ServerSessionId {get; set;}
        public DomainConnectionFactory ConnectionFactory {get; set;}
        public AgentSelector RegisterAgents => new AgentSelector(this);

        [NonSerialized]
        internal List<DomainAgentSessionHandle> agentSessions;


        protected ServerContextBase()
        {
            agentSessions = new List<DomainAgentSessionHandle>();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            agentSessions = new List<DomainAgentSessionHandle>();
        }

        public virtual void Dispose()
        {
            foreach (var session in agentSessions)
                session.Dispose();

            agentSessions.Clear();
        }
    }
}
