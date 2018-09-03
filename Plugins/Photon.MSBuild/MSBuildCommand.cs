using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Process;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.MSBuild
{
    /// <summary>
    /// Adapter for using MSBuild from the command line.
    /// </summary>
    public class MSBuildCommand : ProcessWrapperBase
    {
        public MSBuildCommand(IDomainContext context = null) : base(context)
        {
            Exe = "msbuild";
        }

        public ProcessResult Run(MSBuildArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return Execute(string.Join(" ", argumentList), cancelToken);
        }

        public async Task<ProcessResult> RunAsync(MSBuildArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return await ExecuteAsync(string.Join(" ", argumentList), cancelToken);
        }
    }
}
