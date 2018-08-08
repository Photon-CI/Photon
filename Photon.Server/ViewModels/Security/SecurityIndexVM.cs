using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewModels.Security
{
    internal class SecurityIndexVM : ServerViewModel
    {
        public bool IsSecurityEnabled {get; set;}


        public SecurityIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Security";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            IsSecurityEnabled = PhotonServer.Instance.ServerConfiguration.Value.Security?.Enabled ?? false;
        }
    }
}
