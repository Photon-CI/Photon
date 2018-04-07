using AnsiConsole;
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
            Map("project").ToAction(new ProjectCommands(context));
            Map("server").ToAction(new ServerCommands(context));
            Map("help", "?").ToAction(OnHelp);

            Map("-ansi").ToProperty(v => ConsoleEx.EnabledAnsi = v, true);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(BuildCommands))
                .Add(typeof(DeployCommands))
                .Add(typeof(ProjectCommands))
                .Add(typeof(ServerCommands))
                .PrintAsync();
        }
    }
}
