using log4net;
using System.ServiceProcess;

namespace Photon.Server
{
    public partial class ServerService : ServiceBase
    {
        private static ILog Log = LogManager.GetLogger(typeof(ServerService));


        public ServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Program.Server.Start();
            Program.Sessions.Start();
            Program.Queue.Start();
        }

        protected override void OnStop()
        {
            Program.Queue.Stop();
            Program.Sessions.Stop();
            Program.Server.Stop();
        }
    }
}
