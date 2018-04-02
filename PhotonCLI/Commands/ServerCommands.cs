using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using Photon.Framework.Extensions;
using System;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Commands
{
    internal class ServerCommands : CommandDictionary<CommandContext>
    {
        public string ServerName {get; set;}
        public string ServerUrl {get; set;}
        public bool ServerPrimary {get; set;}


        public ServerCommands(CommandContext context) : base(context)
        {
            Map("add").ToAction(AddCommand);
            Map("remove").ToAction(RemoveCommand);
            Map("list").ToAction(ListCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-n", "-name").ToProperty(v => ServerName = v);
            Map("-u", "-url").ToProperty(v => ServerUrl = v);
            Map("-p", "-primary").ToProperty<bool?>(v => ServerPrimary = v ?? true);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add("Add", "Add a new Photon Server definition.")
                .Add("Remove", "Remove an existing Photon Server definition.")
                .Add("List", "List all defined Photon Server definitions.")
                .PrintAsync();
        }

        public async Task AddCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("Add", "Add a new Photon Server definition.")
                    .Add("-name    | -n", "The name of the Server.")
                    .Add("-url     | -u", "The URL of the Server.")
                    .Add("-primary | -p", "Make this the primary server.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ServerName))
                throw new ApplicationException("'-name' is undefined!");

            if (string.IsNullOrEmpty(ServerUrl))
                throw new ApplicationException("'-url' is undefined!");

            try {
                await new ServerAddAction {
                    ServerName = ServerName,
                    ServerUrl = ServerUrl,
                    ServerPrimary = ServerPrimary,
                }.Run(Context);

                ConsoleEx.Out
                    .WriteLine("Server definition added successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to add server definition!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }

        public async Task RemoveCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("Add", "Remove an existing Photon Server definition.")
                    .Add("-name | -n", "The name of the Server.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ServerName))
                throw new ApplicationException("'-name' is undefined!");

            try {
                await new ServerRemoveAction {
                    ServerName = ServerName,
                }.Run(Context);

                ConsoleEx.Out
                    .WriteLine("Server definition removed successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to remove server definition!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }

        public async Task ListCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("List", "List all defined Photon Server definitions.")
                    .PrintAsync();

                return;
            }

            try {
                await new ServerListAction()
                    .Run(Context);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to list server definitions!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
}
