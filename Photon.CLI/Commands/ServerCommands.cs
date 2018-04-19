using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    [Command("Server", "Manage the collection of named Photon Server instances.")]
    internal class ServerCommands : CommandDictionary<CommandContext>
    {
        public string ServerName {get; set;}
        public string ServerUrl {get; set;}
        public bool? ServerPrimary {get; set;}


        public ServerCommands(CommandContext context) : base(context)
        {
            Map("add").ToAction(AddCommand);
            Map("edit").ToAction(EditCommand);
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
                .Add(typeof(ServerCommands), nameof(AddCommand))
                .Add(typeof(ServerCommands), nameof(EditCommand))
                .Add(typeof(ServerCommands), nameof(RemoveCommand))
                .Add(typeof(ServerCommands), nameof(ListCommand))
                .PrintAsync();
        }

        [Command("Add", "Add a new Photon Server definition.")]
        public async Task AddCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(ServerCommands), nameof(AddCommand))
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

        [Command("Edit", "Edit an existing Photon Server definition.")]
        public async Task EditCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(ServerCommands), nameof(EditCommand))
                    .Add("-name    | -n", "The name of the Server.")
                    .Add("-url     | -u", "The URL of the Server.")
                    .Add("-primary | -p", "Make this the primary server.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ServerName))
                throw new ApplicationException("'-name' is undefined!");

            if (string.IsNullOrEmpty(ServerUrl) && !ServerPrimary.HasValue)
                throw new ApplicationException("No changes were specified!");

            try {
                await new ServerEditAction {
                    ServerName = ServerName,
                    ServerUrl = ServerUrl,
                    ServerPrimary = ServerPrimary,
                }.Run(Context);

                ConsoleEx.Out
                    .WriteLine("Server definition modified successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to modify server definition!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }

        [Command("Remove", "Remove an existing Photon Server definition.")]
        public async Task RemoveCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(ServerCommands), nameof(RemoveCommand))
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

        [Command("List", "List all defined Photon Server definitions.")]
        public async Task ListCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(ServerCommands), nameof(ListCommand))
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
