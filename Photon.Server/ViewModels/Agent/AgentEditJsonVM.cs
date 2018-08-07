using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.Agent
{
    internal class AgentEditJsonVM : ServerViewModel
    {
        public string AgentId {get; set;}


        public AgentEditJsonVM(IHttpHandler handler) : base(handler) {}
    }
}
