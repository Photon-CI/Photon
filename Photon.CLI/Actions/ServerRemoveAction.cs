using Photon.CLI.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class ServerRemoveAction
    {
        public string ServerName {get; set;}


        public async Task Run(CommandContext context)
        {
            if (!context.Servers.RemoveByName(ServerName))
                throw new ApplicationException($"Failed to remove server definition '{ServerName}'!");

            await Task.Run(() => context.Servers.Save());
        }
    }
}
