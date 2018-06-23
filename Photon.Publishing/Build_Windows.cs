using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing
{
    public class Build_Windows : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution();
            await UnitTest();
        }

        private async Task BuildSolution()
        {
            var msbuild_exe = Context.AgentVariables["global"]["msbuild_exe"];

            await Context.RunCommandLineAsync(
                //".\\bin\\msbuild.cmd", "/m", "/v:m",
                $"\"{msbuild_exe}\"",
                "Photon.sln", "/m", "/v:m",
                "/p:Configuration=Release",
                "/p:Platform=\"Any CPU\"",
                "/t:Rebuild");
        }

        private async Task UnitTest()
        {
            var nunit_exe = Context.AgentVariables["global"]["nunit_exe"];

            await Context.RunCommandLineAsync(
                $"\"{nunit_exe}\"",
                "\"Photon.Tests\\bin\\Release\\Photon.Tests.dll\"",
                "--where=\"cat == 'unit'\"");
        }
    }
}
