using log4net;
using Photon.Server.Internal;
using System.ServiceProcess;

namespace Photon.Server
{
    public partial class ServerService : ServiceBase
    {
        private static ILog Log = LogManager.GetLogger(typeof(ServerService));


        public ServerService()
        {
            Program.Server = new PhotonServer();

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.Server.Start();
        }

        protected override void OnStop()
        {
            Program.Server.Stop();
        }
    }
}
