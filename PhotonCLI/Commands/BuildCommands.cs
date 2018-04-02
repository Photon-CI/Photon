using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using Photon.Framework.Extensions;
using System;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Commands
{
    internal class BuildCommands : CommandDictionary<CommandContext>
    {
        public string ProjectName {get; set;}
        public string ProjectRefspec {get; set;}
        public string ScriptName {get; set;}


        public BuildCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-p", "-project").ToProperty(v => ProjectName = v);
            Map("-r", "-refspec").ToProperty(v => ProjectRefspec = v);
            Map("-s", "-script").ToProperty(v => ScriptName = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add("Run", "Run a Build script.")
                .PrintAsync();
        }

        public async Task RunCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter("Run", "Runs a project build script using the specified refspec.")
                    .Add("-project | -p", "The ID of the project.")
                    .Add("-refspec | -r", "The repository branch, commit, or tag.")
                    .Add("-script  | -s", "The name of the build script.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ProjectName))
                throw new ApplicationException("'-project' is undefined!");

            if (string.IsNullOrEmpty(ProjectRefspec))
                throw new ApplicationException("'-refspec' is undefined!");

            if (string.IsNullOrEmpty(ScriptName))
                throw new ApplicationException("'-script' is undefined!");

            ConsoleEx.Out
                .Write($"Running build script ", ConsoleColor.DarkCyan)
                .Write(ScriptName, ConsoleColor.Cyan)
                .Write(" @ ", ConsoleColor.DarkCyan)
                .Write(ProjectRefspec, ConsoleColor.Cyan)
                .Write(" from project ", ConsoleColor.DarkCyan)
                .Write(ProjectName, ConsoleColor.Cyan)
                .WriteLine(".", ConsoleColor.DarkCyan);

            try {
                await new BuildRunAction {
                    ProjectName = ProjectName,
                    ProjectRefspec = ProjectRefspec,
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
