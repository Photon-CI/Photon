using Photon.Agent.ViewModels;
using Photon.Library;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentViewModel : ViewModelBase
    {
        public MasterVM Master {get;}


        public AgentViewModel()
        {
            PageTitle = "Photon Agent";

            Master = new MasterVM();
            SecurityEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;
        }

        protected override void OnBuild()
        {
            Master.Build();
        }
    }

    internal class AgentViewModelAsync : ViewModelAsyncBase
    {
        public MasterVM Master {get;}


        public AgentViewModelAsync()
        {
            PageTitle = "Photon Server";

            Master = new MasterVM();
            SecurityEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;
        }

        protected override async Task OnBuildAsync()
        {
            Master.Build();
        }
    }
}
