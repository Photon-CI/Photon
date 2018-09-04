using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Process;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NUnitPlugin
{
    /// <summary>
    /// Adapter for using MSBuild from the command line.
    /// </summary>
    public class NUnit3Command : ProcessWrapperBase
    {
        public NUnit3Command(IDomainContext context = null) : base(context)
        {
            Exe = "nunit3-console";
        }

        public ProcessResult Run(NUnit3Arguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return Execute(string.Join(" ", argumentList), cancelToken);
        }

        public async Task<ProcessResult> RunAsync(NUnit3Arguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return await ExecuteAsync(string.Join(" ", argumentList), cancelToken);
        }
    }
}
