using Photon.Agent.ViewModels;
using Photon.Library;
using PiServerLite.Http.Handlers;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentViewModel : ViewModelBase
    {
        public IHttpHandler Handler {get;}
        public MasterVM Master {get;}


        public AgentViewModel(IHttpHandler handler)
        {
            this.Handler = handler;

            PageTitle = "Photon Agent";

            Master = new MasterVM();
        }

        protected override void OnBuild()
        {
            SecurityEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;

            Master.Build(Handler);
        }
    }

    internal class AgentViewModelAsync : ViewModelAsyncBase
    {
        public IHttpHandler Handler {get;}
        public MasterVM Master {get;}


        public AgentViewModelAsync(IHttpHandler handler)
        {
            this.Handler = handler;

            PageTitle = "Photon Server";

            Master = new MasterVM();
        }

        protected override async Task OnBuildAsync()
        {
            SecurityEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;

            Master.Build(Handler);
        }
    }
}
