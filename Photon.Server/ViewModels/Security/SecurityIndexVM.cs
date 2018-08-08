using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.Security
{
    internal class SecurityIndexVM : ServerViewModel
    {
        public SecurityIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Security";
        }
    }
}
