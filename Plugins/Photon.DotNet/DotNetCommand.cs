using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Process;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.DotNetPlugin
{
    /// <summary>
    /// Adapter for using MSBuild from the command line.
    /// </summary>
    public class DotNetCommand : ProcessWrapperBase
    {
        public DotNetCommand(IDomainContext context = null) : base(context)
        {
            Exe = "dotnet";
        }

        /// <summary>
        /// Builds a project and all of its dependencies.
        /// </summary>
        public ProcessResult Build(DotNetBuildArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            var argumentList = arguments.GetArguments().ToArray();
            return Execute(string.Join(" ", argumentList), cancelToken);
        }

        /// <summary>
        /// Builds a project and all of its dependencies.
        /// </summary>
        public async Task<ProcessResult> BuildAsync(DotNetBuildArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await RunAsync(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// Packs the code into a NuGet package.
        /// </summary>
        public ProcessResult Pack(DotNetPackArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return Run(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// Packs the code into a NuGet package.
        /// </summary>
        public async Task<ProcessResult> PackAsync(DotNetPackArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await RunAsync(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// Packs the application and its dependencies into a folder for deployment to a hosting system.
        /// </summary>
        public ProcessResult Publish(DotNetPublishArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return Run(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// Packs the application and its dependencies into a folder for deployment to a hosting system.
        /// </summary>
        public async Task<ProcessResult> PublishAsync(DotNetPublishArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await RunAsync(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// Restores the dependencies and tools of a project.
        /// </summary>
        public ProcessResult Restore(DotNetRestoreArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return Run(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// Restores the dependencies and tools of a project.
        /// </summary>
        public async Task<ProcessResult> RestoreAsync(DotNetRestoreArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await RunAsync(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// .NET test driver used to execute unit tests.
        /// </summary>
        public ProcessResult Test(DotNetTestArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return Run(arguments.GetArguments(), cancelToken);
        }

        /// <summary>
        /// .NET test driver used to execute unit tests.
        /// </summary>
        public async Task<ProcessResult> TestAsync(DotNetTestArguments arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await RunAsync(arguments.GetArguments(), cancelToken);
        }
    }
}
