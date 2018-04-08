using Photon.Library.Commands;
using System.Threading.Tasks;

namespace Photon.Server.Commands
{
    internal class RootCommands : CommandDictionary<object>
    {
        public bool Debug {get; private set;}


        public RootCommands() : base(null)
        {
            ActionRequired = false;

            Map("help", "?").ToAction(OnHelp);

            Map("-debug").ToProperty(v => Debug = v, true);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add("-debug", "Run as Console Application.")
                .PrintAsync();
        }
    }
}
