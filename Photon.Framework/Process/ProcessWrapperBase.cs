using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Process
{
    public abstract class ProcessWrapperBase
    {
        protected readonly IDomainContext context;

        /// <summary>
        /// Gets or sets the executable to use when running MSBuild.
        /// </summary>
        public string Exe {get; set;}

        /// <summary>
        /// Gets or sets the optional working directory for executing MSBuild.
        /// </summary>
        public string WorkingDirectory {get; set;}

        /// <summary>
        /// Gets or sets the writer to print to process output to.
        /// </summary>
        public IWriteBlocks Output {get; set;}

        /// <summary>
        /// Gets or sets whether the command run is echoed to the task output stream.
        /// </summary>
        public bool EchoCommand {get; set;}


        protected ProcessWrapperBase(IDomainContext context = null)
        {
            this.context = context;

            EchoCommand = true;
            Output = context?.Output;
        }

        public ProcessResult Run(string arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return Execute(arguments, cancelToken);
        }

        public async Task<ProcessResult> RunAsync(string arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await ExecuteAsync(arguments, cancelToken);
        }

        public ProcessResult Run(IEnumerable<string> arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return Execute(string.Join(" ", arguments), cancelToken);
        }

        public async Task<ProcessResult> RunAsync(IEnumerable<string> arguments, CancellationToken cancelToken = default(CancellationToken))
        {
            return await ExecuteAsync(string.Join(" ", arguments), cancelToken);
        }

        protected ProcessResult Execute(string arguments, CancellationToken cancelToken)
        {
            if (string.IsNullOrEmpty(Exe))
                throw new ApplicationException("Exe is undefined!");

            var info = GetProcessArgs(arguments);

            var result = new ProcessRunner(context) {
                Output = Output,
            }.Run(info, cancelToken);

            if (result.ExitCode != 0) throw new ProcessExitCodeException(result);
            return result;
        }

        protected async Task<ProcessResult> ExecuteAsync(string arguments, CancellationToken cancelToken)
        {
            if (string.IsNullOrEmpty(Exe))
                throw new ApplicationException("Exe is undefined!");

            var info = GetProcessArgs(arguments);

            var result = await new ProcessRunner(context) {
                Output = Output,
            }.RunAsync(info, cancelToken);

            if (result.ExitCode != 0) throw new ProcessExitCodeException(result);
            return result;
        }

        protected ProcessRunInfo GetProcessArgs(string arguments)
        {
            var info = new ProcessRunInfo {
                Filename = Exe,
                Arguments = arguments,
                EchoCommand = EchoCommand,
            };

            if (!string.IsNullOrEmpty(WorkingDirectory))
                info.WorkingDirectory = WorkingDirectory;

            return info;
        }
    }
}
