using Photon.Library;

namespace Photon.Agent.Internal
{
    internal class AgentViewModel : ViewModelBase
    {
        public AgentViewModel()
        {
            PageTitle = "Photon Agent";

            SecurityEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;
        }
    }
}
