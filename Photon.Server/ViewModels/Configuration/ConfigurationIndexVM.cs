using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.Configuration
{
    internal class ConfigurationIndexVM : ServerViewModel
    {
        public ConfigurationIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Configuration";
        }
    }
}
