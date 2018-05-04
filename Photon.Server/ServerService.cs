using log4net;
using Photon.Server.Internal;
using System;
using System.ServiceProcess;

namespace Photon.Server
{
    public partial class ServerService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerService));


        public ServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try {
                PhotonServer.Instance.Start();
            }
            catch (Exception error) {
                Log.Error("Failed to start service!", error);
            }
        }

        protected override void OnStop()
        {
            try {
                PhotonServer.Instance.Stop();
            }
            catch (Exception error) {
                Log.Error("Failed to stop service!", error);
            }
        }
    }
}
