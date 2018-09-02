using Photon.Framework.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Process
{
    [Serializable]
    public class ProcessClient
    {
        private readonly IDomainContext context;


        public ProcessClient(IDomainContext context)
        {
            this.context = context;
        }

        public ProcessResult Run(string command, CancellationToken cancelToken = default(CancellationToken))
        {
            var runner = new ProcessRunner(context);
            var arguments = ProcessRunInfo.FromCommand(command);
            var result = runner.Run(arguments, cancelToken);
            ProcessResult(result);
            return result;
        }

        public ProcessResult Run(ProcessRunInfo info, CancellationToken cancelToken = default(CancellationToken))
        {
            var runner = new ProcessRunner(context);
            var result = runner.Run(info, cancelToken);
            ProcessResult(result);
            return result;
        }

        public async Task<ProcessResult> RunAsync(string command, CancellationToken cancelToken = default(CancellationToken))
        {
            var runner = new ProcessRunner(context);
            var arguments = ProcessRunInfo.FromCommand(command);
            var result = await runner.RunAsync(arguments, cancelToken);
            ProcessResult(result);
            return result;
        }

        public async Task<ProcessResult> RunAsync(ProcessRunInfo info, CancellationToken cancelToken = default(CancellationToken))
        {
            var runner = new ProcessRunner(context);
            var result = await runner.RunAsync(info, cancelToken);
            ProcessResult(result);
            return result;
        }

        private void ProcessResult(ProcessResult result)
        {
            if (result.ExitCode != 0) throw new ApplicationException($"Process returned a non-zero exit code! [{result.ExitCode}]");
        }
    }
}
