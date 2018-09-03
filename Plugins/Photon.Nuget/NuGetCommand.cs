using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Process;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetCommand : ProcessWrapperBase
    {
        private static readonly Regex existsExp;


        static NuGetCommand()
        {
            existsExp = new Regex(@":\s*409\s*\(A package with ID '\S+' and version '\S+' already exists", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public NuGetCommand(IDomainContext context = null) : base(context)
        {
            Exe = "NuGet";
        }

        public ProcessResult Run(NuGetRestoreArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return Execute(string.Join(" ", argumentList), cancelToken);
        }

        public async Task<ProcessResult> RunAsync(NuGetRestoreArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return await ExecuteAsync(string.Join(" ", argumentList), cancelToken);
        }

        public ProcessResult Run(NuGetPackArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return Execute(string.Join(" ", argumentList), cancelToken);
        }

        public async Task<ProcessResult> RunAsync(NuGetPackArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return await ExecuteAsync(string.Join(" ", argumentList), cancelToken);
        }

        public ProcessResult Run(NuGetPushArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();

            try {
                return Execute(string.Join(" ", argumentList), cancelToken);
            }
            catch (ProcessExitCodeException error) {
                if (error.Result != null && existsExp.IsMatch(error.Result.Error)) {
                    Output?.WriteLine("Package already exists.", ConsoleColor.DarkYellow);
                    return error.Result;
                }

                throw;
            }
        }

        public async Task<ProcessResult> RunAsync(NuGetPushArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();

            try {
                return await ExecuteAsync(string.Join(" ", argumentList), cancelToken);
            }
            catch (ProcessExitCodeException error) {
                if (error.Result != null && existsExp.IsMatch(error.Result.Error)) {
                    Output?.WriteLine("Package already exists.", ConsoleColor.DarkYellow);
                    return error.Result;
                }

                throw;
            }
        }
    }
}
