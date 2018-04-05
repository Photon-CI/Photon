using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using Photon.Framework.Extensions;
using System;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Commands
{
    [Command("Build", "Run Build scripts to create new packages.")]
    internal class BuildCommands : CommandDictionary<CommandContext>
    {
        public string ProjectId {get; set;}
        public string GitRefspec {get; set;}
        public string AssemblyFile {get; set;}
        public string ScriptName {get; set;}
        public string StartFile {get; set;}


        public BuildCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-p", "-project").ToProperty(v => ProjectId = v);
            Map("-r", "-refspec").ToProperty(v => GitRefspec = v);
            Map("-a", "-assembly").ToProperty(v => AssemblyFile = v);
            Map("-s", "-script").ToProperty(v => ScriptName = v);
            Map("-f", "-file").ToProperty(v => StartFile = v);
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
                    .Add("-project  | -p", "The ID of the project.")
                    .Add("-refspec  | -r", "The repository branch, commit, or tag.")
                    .Add("-assembly | -a", "The assembly filename, relative to the repository root.")
                    .Add("-script   | -s", "The name of the build script.")
                    .Add("-file     | -f", "A json file specifying the build parameters.")
                    .PrintAsync();

                return;
            }

            if (!string.IsNullOrEmpty(StartFile)) {
                ConsoleEx.Out
                    .Write("Running Build-Script using file ", ConsoleColor.DarkCyan)
                    .Write(StartFile, ConsoleColor.Cyan)
                    .WriteLine(".", ConsoleColor.DarkCyan);
            }
            else {
                if (string.IsNullOrEmpty(ProjectId))
                    throw new ApplicationException("'-project' is undefined!");

                if (string.IsNullOrEmpty(GitRefspec))
                    throw new ApplicationException("'-refspec' is undefined!");

                if (string.IsNullOrEmpty(AssemblyFile))
                    throw new ApplicationException("'-assembly' is undefined!");

                if (string.IsNullOrEmpty(ScriptName))
                    throw new ApplicationException("'-script' is undefined!");

                ConsoleEx.Out
                    .Write("Running Build-Script ", ConsoleColor.DarkCyan)
                    .Write(ScriptName, ConsoleColor.Cyan)
                    .Write(" @ ", ConsoleColor.DarkCyan)
                    .Write(GitRefspec, ConsoleColor.Cyan)
                    .Write(" from project ", ConsoleColor.DarkCyan)
                    .Write(ProjectId, ConsoleColor.Cyan)
                    .WriteLine(".", ConsoleColor.DarkCyan);
            }

            try {
                await new BuildRunAction {
                    ProjectName = ProjectId,
                    GitRefspec = GitRefspec,
                    AssemblyFile = AssemblyFile,
                    ScriptName = ScriptName,
                    StartFile = StartFile,
                }.Run(Context);

                ConsoleEx.Out
                    .WriteLine("Build-Script completed successfully.", ConsoleColor.Green);
            }
            catch (Exception error) {
                ConsoleEx.Out
                    .WriteLine("Build-Script Failed!", ConsoleColor.Red)
                    .WriteLine(error.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
}
