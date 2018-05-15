using Photon.Framework.Server;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    internal class TestDeployTask : IDeployScript
    {
        public IServerDeployContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            var agents = Context.RegisterAgents.Environment(Context.EnvironmentName);

            //...
        }
    }
}
