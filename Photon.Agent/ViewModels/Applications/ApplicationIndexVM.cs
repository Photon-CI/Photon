using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewModels.Applications
{
    internal class ApplicationIndexVM : AgentViewModel
    {
        public ApplicationIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Applications";
        }
    }
}
