using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels.Configuration
{
    internal class ConfigurationIndexVM : AgentViewModel
    {
        public ConfigurationIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Configuration";
        }
    }
}
