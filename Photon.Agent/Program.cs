using System.ServiceProcess;

namespace Photon.Agent
{
    internal static class Program
    {
        public static void Main()
        {
            ServiceBase.Run(new[] {
                new AgentService()
            });
        }
    }
}
