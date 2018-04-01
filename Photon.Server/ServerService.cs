using Photon.Server.Internal;
using System.ServiceProcess;

namespace Photon.Server
{
    public partial class ServerService : ServiceBase
    {
        public ServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            PhotonServer.Instance.Start();
        }

        protected override void OnStop()
        {
            PhotonServer.Instance.Stop();
        }
    }
}
