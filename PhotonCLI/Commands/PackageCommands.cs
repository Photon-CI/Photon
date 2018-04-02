using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using Photon.Framework.Extensions;
using System;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Commands
{
    internal class PackageCommands : CommandDictionary<CommandContext>
    {
        public string ContentDirectory {get; set;}


        public PackageCommands(CommandContext context) : base(context)
        {
            Map("pack").ToAction(PackCommand);
            Map("unpack").ToAction(UnpackCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-d", "-directory").ToProperty(v => ContentDirectory = v);
            //...
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add("Pack", "Create a package from directory content.")
                .Add("Unpack", "Copy package content to a directory.")
                .PrintAsync();
        }

        public async Task PackCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("Pack", "Creates a package from directory content.")
                    .Add("-project   | -p", "The ID of the project.")
                    .Add("-version   | -v", "The version of the project package.")
                    .Add("-directory | -d", "The directory to retrieve content from.")
                    .PrintAsync();

                return;
            }

            try {
                await new PackagePackAction {
                    ContentDirectory = ContentDirectory,
                }.Run();

                ConsoleEx.Out
                    .WriteLine("Package created successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to create Package!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }

        public async Task UnpackCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("Unpack", "Copies the contents of a package to a directory.")
                    .Add("-project | -p", "The ID of the project.")
                    .Add("-version | -v", "The version of the project package.")
                    .Add("-directory | -d", "The directory to copy content to.")
                    .PrintAsync();

                return;
            }

            try {
                await new PackageUnpackAction {
                    ContentDirectory = ContentDirectory,
                }.Run();

                ConsoleEx.Out
                    .WriteLine("Package contents copied successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to copy Package contents!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
}
