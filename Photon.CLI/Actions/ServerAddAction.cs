using Photon.CLI.Internal;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class ServerAddAction
    {
        public string ServerName {get; set;}
        public string ServerUrl {get; set;}
        public bool? ServerPrimary {get; set;}


        public async Task Run(CommandContext context)
        {
            var definition = new PhotonServerDefinition {
                Name = ServerName,
                Url = ServerUrl,
                Primary = ServerPrimary ?? false,
            };

            context.Servers.Add(definition);

            if (ServerPrimary ?? false)
                context.Servers.SetPrimary(ServerName);

            await Task.Run(() => context.Servers.Save());
        }
    }
}
