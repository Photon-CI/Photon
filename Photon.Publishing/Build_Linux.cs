using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Photon.MSBuildPlugin;

namespace Photon.Publishing
{
    public class Build_Linux : IBuildTask
    {
        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution(token);
            // TODO: Test
        }

        private async Task BuildSolution(CancellationToken token)
        {
            var msbuild = new MSBuildCommand(Context) {
                Exe = "msbuild",
                WorkingDirectory = Context.ContentDirectory,
            };

            var buildArgs = new MSBuildArguments {
                ProjectFile = "Photon.sln",
                Targets = {"Rebuild"},
                Properties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "Any CPU",
                },
                Verbosity = MSBuildVerbosityLevels.Minimal,
                NodeReuse = false,
                NoLogo = true,
                MaxCpuCount = 0,
            };

            await msbuild.RunAsync(buildArgs, token);
        }
    }
}
