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
            Map("run").ToAction(UpdateCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-s", "-server").ToProperty(v => Server = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(UpdateCommands), nameof(UpdateCommand))
                .PrintAsync();
        }

        [Command("Update", "Updates all agents attached to the server.")]
        public async Task UpdateCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(UpdateCommands), nameof(UpdateCommand))
                    .Add("-server  | -s", "The name of the target Photon Server.")
                    .PrintAsync();

                return;
            }

            var updateAction = new UpdateRunAction {
                ServerName = Server,
            };

            await updateAction.Run(Context);
        }
    }
}
