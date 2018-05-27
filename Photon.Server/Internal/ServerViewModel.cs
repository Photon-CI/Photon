using Photon.Library;

namespace Photon.Server.Internal
{
    internal class ServerViewModel : ViewModelBase
    {
        public ServerViewModel()
        {
            PageTitle = "Photon Server";

            SecurityEnabled = PhotonServer.Instance.ServerConfiguration.Value.Security?.Enabled ?? false;
        }
    }
}
