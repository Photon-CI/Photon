using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    [Command("Update", "Automate software updates.")]
    internal class UpdateCommands : CommandDictionary<CommandContext>
    {
        public string Server {get; set;}


        public UpdateCommands(CommandContext context) : base(context)
        {
            Map("server").ToAction(UpdateServerCommand);
            Map("agents").ToAction(UpdateAgentsCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-s", "-server").ToProperty(v => Server = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(UpdateCommands), nameof(UpdateServerCommand))
                .Add(typeof(UpdateCommands), nameof(UpdateAgentsCommand))
                .PrintAsync();
        }

        [Command("Server", "Updates the server.")]
        public async Task UpdateServerCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(UpdateCommands), nameof(UpdateServerCommand))
                    .Add("-server  | -s", "The name of the target Photon Server.")
                    .PrintAsync();

                return;
            }

            var updateAction = new UpdateServerAction {
                ServerName = Server,
            };

            await updateAction.Run(Context);
        }

        [Command("Agents", "Updates all agents attached to the server.")]
        public async Task UpdateAgentsCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(UpdateCommands), nameof(UpdateAgentsCommand))
                    .Add("-server  | -s", "The name of the target Photon Server.")
                    .PrintAsync();

                return;
            }

            var updateAction = new UpdateAgentsAction {
                ServerName = Server,
            };

            await updateAction.Run(Context);
        }
    }
}
