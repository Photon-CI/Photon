using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    public class Build_Windows : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution();
        }

        private async Task BuildSolution()
        {
            var msbuild_exe = Context.AgentVariables["global"]["msbuild_exe"];

            await Context.RunCommandLineAsync(
                //".\\bin\\msbuild.cmd", "/m", "/v:m",
                $"\"{msbuild_exe}\"",
                "Photon.sln", "/m",
                "/p:Configuration=Release",
                "/p:Platform=\"Any CPU\"",
                "/t:Rebuild");
        }
    }
}
