using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using Photon.Publishing.PhotonServer;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    public class PublishTask : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}


        public async Task<TaskResult> RunAsync()
        {
            var tools = new PhotonFrameworkTools(Context);

            await tools.Publish();

            return TaskResult.Ok();
        }
    }
}
