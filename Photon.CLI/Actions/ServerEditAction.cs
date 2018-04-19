using Photon.CLI.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class ServerEditAction
    {
        public string ServerName {get; set;}
        public string ServerUrl {get; set;}
        public bool? ServerPrimary {get; set;}


        public async Task Run(CommandContext context)
        {
            if (!context.Servers.TryGet(ServerName, out var server))
                throw new ApplicationException($"Server definition '{ServerName}' not found!");

            if (ServerUrl != null)
                server.Url = ServerUrl;

            if (ServerPrimary.HasValue)
                server.Primary = ServerPrimary.Value;

            await Task.Run(() => context.Servers.Save());
        }
    }
}
