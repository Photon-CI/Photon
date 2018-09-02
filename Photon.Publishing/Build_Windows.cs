using Photon.Framework.Agent;
using Photon.Framework.Process;
using Photon.Framework.Tasks;
using Photon.MSBuild;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing
{
    public class Build_Windows : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution(token);
            await UnitTest(token);
        }

        private async Task BuildSolution(CancellationToken token)
        {
            var msbuild = new MSBuildCommand(Context) {
                Exe = Context.AgentVariables["global"]["msbuild_exe"],
                WorkingDirectory = Context.ContentDirectory,
            };

            var buildArgs = new MSBuildArguments {
                ProjectFile = "Photon.sln",
                Targets = {"Rebuild"},
                Properties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "Any CPU",
                },
                Verbosity = MSBuildVerbosityLevel.Minimal,
                NodeReuse = false,
                NoLogo = true,
                MaxCpuCount = 0,
            };

            await msbuild.RunAsync(buildArgs, token);
        }

        private async Task UnitTest(CancellationToken token)
        {
            var info = new ProcessRunInfo {
                Filename = Context.AgentVariables["global"]["nunit_exe"],
                Arguments = "\"Photon.Tests\\bin\\Release\\Photon.Tests.dll\" --where=\"cat == 'unit'\"",
            };

            await new ProcessRunner(Context)
                .RunAsync(info, token);
        }
    }
}
