using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using Photon.Framework.Extensions;
using System;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Commands
{
    [Command("Project", "Create and Unpack Project Packages.")]
    internal class ProjectCommands : CommandDictionary<CommandContext>
    {
        public string MetadataFilename {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public string PackageFilename {get; set;}
        public string OutputPath {get; set;}


        public ProjectCommands(CommandContext context) : base(context)
        {
            Map("package").ToAction(PackageCommand);
            Map("unpack").ToAction(UnpackCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-id").ToProperty(v => PackageId = v);
            Map("-m", "-metadata").ToProperty(v => MetadataFilename = v);
            Map("-v", "-version").ToProperty(v => PackageVersion = v);
            Map("-f", "-filename").ToProperty(v => PackageFilename = v);
            Map("-o", "-output").ToProperty(v => OutputPath = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(GetType(), nameof(PackageCommand))
                .Add(GetType(), nameof(UnpackCommand))
                .PrintAsync();
        }

        [Command("Package", "Create a Project Package from metadata.")]
        public async Task PackageCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(GetType(), nameof(PackageCommand))
                    .Add("-metadata | -m", "The Project Package metadata filename.")
                    .Add("-version  | -v", "The version of the Project Package.")
                    .Add("-filename | -f", "The filename of the Project Package to create.")
                    .PrintAsync();

                return;
            }

            try {
                await new ProjectPackageAction {
                    MetadataFilename = MetadataFilename,
                    PackageFilename = PackageFilename,
                    PackageVersion = PackageVersion,
                }.Run();

                ConsoleEx.Out
                    .WriteLine("Project Package created successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to create Project Package!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }

        [Command("Unpack", "Unpackage the contents of a Project Package to a directory.")]
        public async Task UnpackCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(GetType(), nameof(UnpackCommand))
                    .Add("-id          ", "The ID of the Project Package.")
                    .Add("-version | -v", "The version of the Project Package.")
                    .Add("-output  | -o", "The directory to copy package content to.")
                    .PrintAsync();

                return;
            }

            try {
                await new ProjectUnpackAction {
                    PackageId = PackageId,
                    PackageVersion = PackageVersion,
                    OutputPath = OutputPath,
                }.Run();

                ConsoleEx.Out
                    .WriteLine("Project Package successfully unpacked.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Failed to unpack Project Package!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
}
