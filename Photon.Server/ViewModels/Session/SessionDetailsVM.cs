using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.Session
{
    internal class SessionDetailsVM : ServerViewModel
    {
        public string SessionId {get; set;}


        public SessionDetailsVM(IHttpHandler handler) : base(handler) {}
    }
}
