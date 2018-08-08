using Newtonsoft.Json.Linq;
using Photon.Library;

namespace Photon.Agent.Internal.AgentConfiguration
{
    internal class AgentConfigurationManager
    {
        private readonly JsonDynamicDocument agentDocument;

        public AgentConfiguration Value {get; private set;}


        public AgentConfigurationManager()
        {
            agentDocument = new JsonDynamicDocument {
                Filename = Configuration.AgentFile,
            };

            Value = new AgentConfiguration();
        }

        public void Load()
        {
            agentDocument.Load(Document_OnLoad);
        }

        public void Save()
        {
            agentDocument.Update(Document_OnUpdate);
        }

        private void Document_OnLoad(dynamic document)
        {
            Value = document.ToObject<AgentConfiguration>();
        }

        private void Document_OnUpdate(dynamic document)
        {
            var mergeValue = JObject.FromObject(Value, agentDocument.Serializer);
            ((JObject)document).Merge(mergeValue);
        }
    }
}
