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
            //...
        }

        protected override void OnStop()
        {
            //...
        }
    }
}
