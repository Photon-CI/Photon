using System.IO;
using System.Text;
using Newtonsoft.Json;
using Photon.Library;

namespace Photon.Server.Internal.ServerConfiguration
{
    internal class ServerConfigurationManager
    {
        private readonly JsonDynamicDocument serverDocument;

        public ServerConfiguration Value {get; private set;}


        public ServerConfigurationManager()
        {
            serverDocument = new JsonDynamicDocument {
                Filename = Configuration.ServerFile,
            };

            Value = new ServerConfiguration();
        }

        public void Load()
        {
            serverDocument.Load(Document_OnLoad);
        }

        public void Save()
        {
            serverDocument.Update(Document_OnUpdate);
        }

        private void Document_OnLoad(dynamic document)
        {
            Value = document.ToObject<ServerConfiguration>();
        }

        private void Document_OnUpdate(dynamic document)
        {
            // Since Json Merge function does not accept a contract resolver,
            // we must first serialize and deserialize the data.

            dynamic _valueX;
            var buffer = new StringBuilder();

            using (var writer = new StringWriter(buffer)) {
                serverDocument.Serializer.Serialize(writer, Value);
            }

            using (var reader = new StringReader(buffer.ToString()))
            using (var jsonReader = new JsonTextReader(reader)) {
                _valueX = serverDocument.Serializer.Deserialize(jsonReader);
            }

            document.Merge(_valueX);
        }
    }
}
