namespace Photon.Server.Internal.ServerConfiguration
{
    internal class ServerConfiguration
    {
        public string Name {get; set;}
        public ServerHttpDefinition Http {get; set;}
        public ServerSecurityDefinition Security {get; set;}


        public ServerConfiguration()
        {
            Http = new ServerHttpDefinition {
                Host = "*",
                Port = 8082,
                Path = "/photon/server",
            };
        }
    }
}
