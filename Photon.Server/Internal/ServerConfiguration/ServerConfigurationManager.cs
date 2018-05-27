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
            document.Merge(Value);
        }
    }
}
