using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    [Command("Build", "Run Build scripts to create new packages.")]
    internal class BuildCommands : CommandDictionary<CommandContext>
    {
        public string Server {get; set;}
        public string ProjectId {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public bool Deploy {get; set;}
        public string Environment {get; set;}


        public BuildCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-s", "-server").ToProperty(v => Server = v);
            Map("-p", "-project").ToProperty(v => ProjectId = v);
            Map("-t", "-task").ToProperty(v => TaskName = v);
            Map("-r", "-refspec").ToProperty(v => GitRefspec = v);
            Map("-d", "-deploy").ToProperty(v => Deploy = v, true);
            Map("-e", "-environment").ToProperty(v => Environment = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(BuildCommands), nameof(RunCommand))
                .PrintAsync();
        }

        [Command("Run", "Runs a project build task using the specified refspec.")]
        public async Task RunCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(BuildCommands), nameof(RunCommand))
                    .Add("-server  | -s", "The name of the target Photon Server.")
                    .Add("-project | -p", "The name of the Project to build.")
                    .Add("-task    | -t", "The name of the Task to build.")
                    .Add("-refspec | -r", "The repository branch, commit, or tag.")
                    .Add("-deploy  | -d", "Deploy all published Project Packages upon successful build.")
                    .Add("-env     | -e", "The environment to deploy to.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(ProjectId))
                throw new ApplicationException("'-project' is undefined!");

            if (string.IsNullOrEmpty(TaskName))
                throw new ApplicationException("'-task' is undefined!");

            var buildAction = new BuildRunAction {
                ServerName = Server,
                ProjectId = ProjectId,
                TaskName = TaskName,
                GitRefspec = GitRefspec,
            };

            await buildAction.Run(Context);

            var successful = buildAction.Result != null && (buildAction.Result.Result?.Successful ?? false);

            if (successful && Deploy) {
                foreach (var package in buildAction.Result.ProjectPackages) {
                    ConsoleEx.Out.Write("Deploying package ", ConsoleColor.DarkCyan)
                        .Write(package.PackageId, ConsoleColor.Cyan)
                        .Write(" @ ", ConsoleColor.DarkCyan)
                        .Write(package.PackageVersion, ConsoleColor.Cyan)
                        .Write(" to ", ConsoleColor.DarkCyan)
                        .Write(Environment, ConsoleColor.Cyan)
                        .WriteLine("...", ConsoleColor.DarkCyan);

                    await new DeployRunAction {
                        ServerName = Server,
                        ProjectId = ProjectId,
                        ProjectPackageId = package.PackageId,
                        ProjectPackageVersion = package.PackageVersion,
                        Environment = Environment,
                    }.Run(Context);
                }
            }
        }
    }
}
