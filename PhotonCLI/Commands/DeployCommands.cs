using AnsiConsole;
using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    [Command("Deploy", "Run Deploy scripts from existing packages.")]
    internal class DeployCommands : CommandDictionary<CommandContext>
    {
        public string ServerName {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        //public string ScriptName {get; set;}


        public DeployCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-server").ToProperty(v => ServerName = v);
            Map("-id").ToProperty(v => ProjectPackageId = v);
            Map("-v", "-version").ToProperty(v => ProjectPackageVersion = v);
            //Map("-s", "-script").ToProperty(v => ScriptName = v);
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
                    .Add("-server      ", "The name of the Server instance. Defaults to primary server.")
                    .Add("-id          ", "The ID of the project package.")
                    .Add("-version | -v", "The version of the project package.")
                    //.Add("-script  | -s", "The name of the deploy script.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ProjectPackageId))
                throw new ApplicationException("'-id' is undefined!");

            if (string.IsNullOrEmpty(ProjectPackageVersion))
                throw new ApplicationException("'-version' is undefined!");

            //if (string.IsNullOrEmpty(ScriptName))
            //    throw new ApplicationException("'-script' is undefined!");

            ConsoleEx.Out
                .Write("Deploying Project Package ", ConsoleColor.DarkCyan)
                .Write($"{ProjectPackageId}.{ProjectPackageVersion}", ConsoleColor.Cyan)
                .WriteLine(".", ConsoleColor.DarkCyan);

            await new DeployRunAction {
                ServerName = ServerName,
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
                //ScriptName = ScriptName,
            }.Run(Context);

            ConsoleEx.Out
                .WriteLine("Script completed successfully.", ConsoleColor.Green);
        }
    }
}
