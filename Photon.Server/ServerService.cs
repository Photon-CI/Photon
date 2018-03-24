using log4net;
using Photon.Server.Internal;
using System.ServiceProcess;

namespace Photon.Server
{
    public partial class ServerService : ServiceBase
    {
        private static ILog Log = LogManager.GetLogger(typeof(ServerService));

        private readonly PhotonServer server;


        public ServerService()
        {
            server = new PhotonServer();

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            server.Start();
        }

        protected override void OnStop()
        {
            server.Stop();
        }
    }
}
