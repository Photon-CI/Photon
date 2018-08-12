using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    [Command("Update", "Automated Server, Agent, and CLI software updates.")]
    internal class UpdateCommands : CommandDictionary<CommandContext>
    {
        public string Server {get; set;}
        public string AgentNames {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}


        public UpdateCommands(CommandContext context) : base(context)
        {
            Map("server").ToAction(UpdateServerCommand);
            Map("agents").ToAction(UpdateAgentsCommand);
            Map("self").ToAction(UpdateSelfCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-s", "-server").ToProperty(v => Server = v);
            Map("-n", "-names").ToProperty(v => AgentNames = v);
            Map("-u", "-user").ToProperty(v => Username = v);
            Map("-p", "-pass").ToProperty(v => Password = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(UpdateCommands), nameof(UpdateServerCommand))
                .Add(typeof(UpdateCommands), nameof(UpdateAgentsCommand))
                .Add(typeof(UpdateCommands), nameof(UpdateSelfCommand))
                .PrintAsync();
        }

        [Command("Server", "Updates the server.")]
        public async Task UpdateServerCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(UpdateCommands), nameof(UpdateServerCommand))
                    .Add("-server  | -s", "The name of the target Photon Server.")
                    .Add("-user    | -u", "The optional username.")
                    .Add("-pass    | -p", "The optional password.")
                    .PrintAsync();

                return;
            }

            var updateAction = new UpdateServerAction {
                ServerName = Server,
                Username = Username,
                Password = Password,
            };

            await updateAction.Run(Context);
        }

        [Command("Agents", "Updates all agents attached to the server.")]
        public async Task UpdateAgentsCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(UpdateCommands), nameof(UpdateAgentsCommand))
                    .Add("-server | -s", "The name of the target Photon Server.")
                    .Add("-names  | -n", "An optional list of Agent names, separated by ';'. Supports '*' wildchar.")
                    .PrintAsync();

                return;
            }

            var updateAction = new UpdateAgentsAction {
                ServerName = Server,
                AgentNames = AgentNames,
            };

            await updateAction.Run(Context);
        }

        [Command("Self", "Updates the CLI application.")]
        public async Task UpdateSelfCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(UpdateCommands), nameof(UpdateSelfCommand))
                    .PrintAsync();

                return;
            }

            var updateAction = new UpdateSelfAction();

            await updateAction.Run(Context);
        }
    }
}
