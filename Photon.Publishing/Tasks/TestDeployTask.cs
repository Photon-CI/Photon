using Photon.Framework.Server;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    internal class TestDeployTask : IDeployScript
    {
        public async Task RunAsync(IServerDeployContext context)
        {
            var agents = context.RegisterAgents.Environment(context.EnvironmentName);

            //...
        }
    }
}
