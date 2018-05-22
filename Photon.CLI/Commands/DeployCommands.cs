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
        public string ProjectId {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        //public string ScriptName {get; set;}
        public string Environment {get; set;}


        public DeployCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-server").ToProperty(v => ServerName = v);
            Map("-project", "-p").ToProperty(v => ProjectId = v);
            Map("-id").ToProperty(v => ProjectPackageId = v);
            Map("-v", "-version").ToProperty(v => ProjectPackageVersion = v);
            //Map("-s", "-script").ToProperty(v => ScriptName = v);
            Map("-e", "-environment").ToProperty(v => Environment = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(DeployCommands), nameof(RunCommand))
                .PrintAsync();
        }

        public async Task RunCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(DeployCommands), nameof(RunCommand))
                    .Add("-server      ", "The name of the Server instance. Defaults to primary server.")
                    .Add("-id          ", "The ID of the project package.")
                    .Add("-version | -v", "The version of the project package.")
                    //.Add("-script  | -s", "The name of the deploy script.")
                    .Add("-env     | -e", "The environment to deploy to.")
                    .Add("-project | -p", "[Optional] Overrides the ID of the project.")
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
                .Write($"{ProjectPackageId}.{ProjectPackageVersion}", ConsoleColor.Cyan);

            if (!string.IsNullOrEmpty(Environment)) {
                ConsoleEx.Out
                    .Write(" to environment ", ConsoleColor.DarkCyan)
                    .Write(Environment, ConsoleColor.Cyan);
            }

            ConsoleEx.Out
                .WriteLine(".", ConsoleColor.DarkCyan);

            await new DeployRunAction {
                ServerName = ServerName,
                ProjectId = ProjectId,
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
                //ScriptName = ScriptName,
                Environment = Environment,
            }.Run(Context);

            //ConsoleEx.Out
            //    .WriteLine("Deployment completed successfully.", ConsoleColor.Green);
        }
    }
}
