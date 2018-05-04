using System;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System.Threading.Tasks;
using Photon.CLI.Actions;

namespace Photon.CLI.Commands
{
    [Command("Log", "Reads the latest log files from nodes.")]
    internal class LogCommands : CommandDictionary<CommandContext>
    {
        public string ServerName {get; set;}
        public string AgentName {get; set;}


        public LogCommands(CommandContext context) : base(context)
        {
            Map("server").ToAction(ServerCommand);
            Map("agent").ToAction(AgentCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-s", "-server").ToProperty(v => ServerName = v);
            Map("-a", "-agent").ToProperty(v => AgentName = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(LogCommands), nameof(ServerCommand))
                .Add(typeof(LogCommands), nameof(AgentCommand))
                .PrintAsync();
        }

        [Command("Server", "Reads the latest log file from the specified server.")]
        public async Task ServerCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(LogCommands), nameof(ServerCommand))
                    .Add("-server, -s", "The name of the Photon Server instance.")
                    .PrintAsync();

                return;
            }

            var action = new LogServerAction {
                ServerName = ServerName,
            };

            await action.Run(Context);
        }

        [Command("Agent", "Reads the latest log file from the specified agent.")]
        public async Task AgentCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(LogCommands), nameof(AgentCommand))
                    .Add("-agent, -a", "The name of the Photon Agent instance.")
                    .PrintAsync();

                return;
            }

            throw new NotImplementedException();

            //var action = new AgentLogAction {
            //    AgentName = AgentName,
            //};

            //await action.Run(Context);
        }
    }
}
