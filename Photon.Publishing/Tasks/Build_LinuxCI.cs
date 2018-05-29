using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    public class Build_LinuxCI : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution();
        }

        private async Task BuildSolution()
        {
            await Context.RunCommandLineAsync(
                "msbuild", "/v:m",
                "Photon.sln",
                "/p:Configuration=\"Linux\"",
                "/p:Platform=\"Any CPU\"",
                "/t:Rebuild");
        }
    }
}
