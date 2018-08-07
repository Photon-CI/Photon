using Photon.Library;
using Photon.Server.ViewModels;
using PiServerLite.Http.Handlers;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerViewModel : ViewModelBase
    {
        public IHttpHandler Handler {get;}
        public MasterVM Master {get;}


        public ServerViewModel(IHttpHandler handler)
        {
            this.Handler = handler;

            PageTitle = "Photon Server";

            Master = new MasterVM();
        }

        protected override void OnBuild()
        {
            SecurityEnabled = PhotonServer.Instance.ServerConfiguration.Value.Security?.Enabled ?? false;

            Master.Build(Handler);
        }
    }

    internal class ServerViewModelAsync : ViewModelAsyncBase
    {
        public IHttpHandler Handler {get;}
        public MasterVM Master {get;}


        public ServerViewModelAsync(IHttpHandler handler)
        {
            this.Handler = handler;

            PageTitle = "Photon Server";

            Master = new MasterVM();
            SecurityEnabled = PhotonServer.Instance.ServerConfiguration.Value.Security?.Enabled ?? false;
        }

        protected override async Task OnBuildAsync()
        {
            Master.Build(Handler);
        }
    }
}
