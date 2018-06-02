using log4net;
using Photon.Agent.Internal;
using System;
using System.ServiceProcess;

namespace Photon.Agent
{
    public partial class AgentService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentService));


        public AgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try {
                PhotonAgent.Instance.Start();
            }
            catch (Exception error) {
                Log.Fatal("Failed to start service!", error);
            }
        }

        protected override void OnStop()
        {
            try {
                PhotonAgent.Instance.Stop();
            }
            catch (Exception error) {
                Log.Error("Failed to stop service!", error);
            }
        }
    }
}
