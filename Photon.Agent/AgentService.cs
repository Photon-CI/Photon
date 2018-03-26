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
            Program.Agent.Start();
        }

        protected override void OnStop()
        {
            Program.Agent.Stop();
        }
    }
}
