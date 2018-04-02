using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using Photon.Framework.Extensions;
using System;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Commands
{
    internal class DeployCommands : CommandDictionary<CommandContext>
    {
        public string ProjectName {get; set;}
        public string ProjectVersion {get; set;}
        public string ScriptName {get; set;}


        public DeployCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-p", "-project").ToProperty(v => ProjectName = v);
            Map("-v", "-version").ToProperty(v => ProjectVersion = v);
            Map("-s", "-script").ToProperty(v => ScriptName = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add("Run", "Run a Deploy script.")
                .PrintAsync();
        }

        public async Task RunCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("Run", "Runs a project deployment script using the specified package version.")
                    .Add("-project | -p", "The ID of the project.")
                    .Add("-version | -v", "The version of the project package.")
                    .Add("-script | -s", "The name of the deploy script.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ProjectName))
                throw new ApplicationException("'-project' is undefined!");

            if (string.IsNullOrEmpty(ProjectVersion))
                throw new ApplicationException("'-version' is undefined!");

            if (string.IsNullOrEmpty(ScriptName))
                throw new ApplicationException("'-script' is undefined!");

            ConsoleEx.Out
                .Write($"Running deploy script ", ConsoleColor.DarkCyan)
                .Write(ScriptName, ConsoleColor.Cyan)
                .Write(" @ ", ConsoleColor.DarkCyan)
                .Write(ProjectVersion, ConsoleColor.Cyan)
                .Write(" from project ", ConsoleColor.DarkCyan)
                .Write(ProjectName, ConsoleColor.Cyan)
                .WriteLine(".", ConsoleColor.DarkCyan);

            try {
                await new DeployRunAction {
                    ProjectName = ProjectName,
                    ProjectVersion = ProjectVersion,
                    ScriptName = ScriptName,
                }.Run();

                ConsoleEx.Out
                    .WriteLine("Script completed successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Script Run Failed!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
}
