using Photon.Agent.Internal;
using System.ServiceProcess;

namespace Photon.Agent
{
    public partial class AgentService : ServiceBase
    {
        public AgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            PhotonAgent.Instance.Start();
        }

        protected override void OnStop()
        {
            PhotonAgent.Instance.Stop();
        }
    }
}
