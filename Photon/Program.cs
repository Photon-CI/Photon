using System.ServiceProcess;

namespace Photon.Server
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            ServiceBase.Run(new [] {
                new ServerService(),
            });

            return 0;
        }
    }
}
