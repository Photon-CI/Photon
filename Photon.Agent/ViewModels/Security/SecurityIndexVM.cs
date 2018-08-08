using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels.Security
{
    internal class SecurityIndexVM : AgentViewModel
    {
        public SecurityIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Security";
        }
    }
}
