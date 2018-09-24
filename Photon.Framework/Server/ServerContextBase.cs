using Photon.Framework.AgentConnection;
using Photon.Framework.Domain;
using System;
using Newtonsoft.Json;

namespace Photon.Framework.Server
{
    public abstract class ServerContextBase : DomainContextBase, IServerContext
    {
        public ServerAgent[] Agents {get; set;}
        public string ServerSessionId {get; set;}
        public IAgentConnectionClient ConnectionFactory {get; set;}
        public WorkerAgentSelector RegisterAgents => new WorkerAgentSelector(ConnectionFactory);

        //[NonSerialized]
        //[JsonIgnore]
        //internal WorkerAgentConnectionCollection agentConnections;


        protected ServerContextBase()
        {
            //agentConnections = new AgentConnectionCollection();
        }

        public virtual void Dispose()
        {
            //agentConnections?.Dispose();
        }
    }
}
