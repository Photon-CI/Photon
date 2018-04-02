using Photon.CLI.Internal;
using Photon.CLI.Internal.Commands;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    internal class RootCommands : CommandDictionary<CommandContext>
    {
        public RootCommands(CommandContext context) : base(context)
        {
            Map("build").ToAction(new BuildCommands(context));
            Map("deploy").ToAction(new DeployCommands(context));
            Map("package").ToAction(new PackageCommands(context));
            Map("server").ToAction(new ServerCommands(context));
            Map("help", "?").ToAction(OnHelp);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add("Build", "Run Build scripts to create new packages.")
                .Add("Deploy", "Run Deploy scripts from existing packages.")
                .Add("Package", "Create and Expand packages.")
                .Add("Server", "Manage the collection of named Photon Server instances.")
                .PrintAsync();
        }
    }
}
