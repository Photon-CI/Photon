namespace Photon.Server.Internal.ServerConfiguration
{
    internal class ServerConfiguration
    {
        public string Name {get; set;}
        public ServerHttpConfiguration Http {get; set;}
        public ServerSecurityConfiguration Security {get; set;}


        public ServerConfiguration()
        {
            Http = new ServerHttpConfiguration {
                Host = "*",
                Port = 8080,
                Path = "/photon/server",
            };

            Security = new ServerSecurityConfiguration {
                Enabled = false,
            };
        }
    }
}
