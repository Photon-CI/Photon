using log4net;
using Newtonsoft.Json.Linq;
using Photon.Framework.Server;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Photon.Server.Internal.ServerAgents
{
    internal class ServerAgentManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerAgentManager));

        private readonly JsonDynamicDocument agentsDocument;
        private readonly ConcurrentDictionary<string, ServerAgent> agentCollection;

        public IEnumerable<ServerAgent> All => agentCollection.Values;


        public ServerAgentManager()
        {
            agentsDocument = new JsonDynamicDocument {
                Filename = Configuration.ServerFile,
            };

            agentCollection = new ConcurrentDictionary<string, ServerAgent>();
        }

        public void Load()
        {
            agentsDocument.Load(Document_OnLoad);
        }

        public bool TryGet(string id, out ServerAgent agent)
        {
            return agentCollection.TryGetValue(id, out agent);
        }

        public void Remove(string id)
        {
            agentCollection.TryRemove(id, out var _);

            agentsDocument.Remove(d => Document_OnRemove(d, id));
        }

        public void SaveAgent(ServerAgent agent, string prevId = null)
        {
            if (prevId != null) {
                agentCollection.TryRemove(prevId, out var _);
            }

            agentCollection.AddOrUpdate(agent.Id, agent, (k, a) => {
                a.Id = agent.Id;
                a.Name = agent.Name;
                a.TcpHost = agent.TcpHost;
                a.TcpPort = agent.TcpPort;
                a.Roles = agent.Roles;
                return a;
            });

            agentsDocument.Update(d => Document_OnUpdate(d, agent, prevId));
        }

        private void Document_OnLoad(JObject document)
        {
            if (!(document.GetValue("agents") is JArray agentArray)) return;

            foreach (var agentDef in agentArray) {
                var agent = agentDef.ToObject<ServerAgent>();

                if (string.IsNullOrEmpty(agent?.Id)) {
                    Log.Warn($"Unable to load agent definition '{agent?.Name}'! Agent 'id' is undefined.");
                    continue;
                }

                agentCollection[agent.Id] = agent;
            }
        }

        private void Document_OnUpdate(JObject document, ServerAgent agent, string prevId)
        {
            if (!(document.SelectToken("agents") is JArray agentArray)) {
                agentArray = new JArray();
                document.Add("agents", agentArray);

                var token = JObject.FromObject(agent);
                agentArray.Add(token);
            }
            else {
                var found = false;
                foreach (var agentDef in agentArray) {
                    var _agent = agentDef.ToObject<ServerAgent>();

                    var _id = prevId ?? agent.Id;
                    if (!string.Equals(_agent.Id, _id)) continue;

                    var agentToken = JToken.FromObject(agent);
                    agentDef.Replace(agentToken);
                    found = true;
                    break;
                }

                if (!found) {
                    var agentToken = JToken.FromObject(agent);
                    agentArray.Add(agentToken);
                }
            }
        }

        private bool Document_OnRemove(JObject document, string id)
        {
            if (!(document.SelectToken("agents") is JArray agentArray))
                return false;

            foreach (var agentDef in agentArray) {
                var _agent = agentDef.ToObject<ServerAgent>();
                if (!string.Equals(_agent.Id, id)) continue;

                agentDef.Remove();
                return true;
            }

            return false;
        }
    }
}
