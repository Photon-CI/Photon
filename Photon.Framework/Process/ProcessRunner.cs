using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SysProcess = System.Diagnostics.Process;

namespace Photon.Framework.Process
{
    public class ProcessRunner
    {
        private readonly IDomainContext context;

        public IWriteBlocks Output {get; set;}


        public ProcessRunner(IDomainContext context = null)
        {
            this.context = context;

            Output = context?.Output;
        }

        public SysProcess Start(ProcessRunInfo info)
        {
            if (info.Filename == null) throw new ArgumentNullException(nameof(info.Filename));

            var startInfo = new ProcessStartInfo {
                FileName = info.Filename,
                Arguments = info.Arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (!string.IsNullOrEmpty(info.WorkingDirectory)) {
                startInfo.WorkingDirectory = info.WorkingDirectory;

                var parts = info.Filename.Split(Path.DirectorySeparatorChar);
                var firstPart = parts.FirstOrDefault();

                if (firstPart == "." || firstPart == "..") {
                    var _p = Path.Combine(info.WorkingDirectory, info.Filename);
                    startInfo.FileName = Path.GetFullPath(_p);
                }
            }

            foreach (var key in info.EnvironmentVariables.Keys)
                startInfo.EnvironmentVariables[key] = info.EnvironmentVariables[key];

            Output?.WriteActionBlock(context, $"Run Command: {info.Filename} {info.Arguments}");

            return SysProcess.Start(startInfo);
        }

        public ProcessResult Run(ProcessRunInfo info, CancellationToken token = default(CancellationToken))
        {
            using (var process = Start(info)) {
                if (process == null)
                    throw new ApplicationException("Failed to start process!");

                var p = process;
                using (token.Register(() => p.Kill())) {
                    var readOutTask = ReadToOutput(process.StandardOutput, ConsoleColor.Gray, token);
                    var readErrorTask = ReadToOutput(process.StandardError, ConsoleColor.DarkYellow, token);

                    process.WaitForExit();
                    Task.WaitAll(readOutTask, readErrorTask);

                    return new ProcessResult {
                        ExitCode = process.ExitCode,
                        Output = readOutTask.Result,
                        Error = readErrorTask.Result,
                    };
                }
            }
        }

        public async Task<ProcessResult> RunAsync(ProcessRunInfo info, CancellationToken token = default(CancellationToken))
        {
            using (var process = Start(info)) {
                if (process == null)
                    throw new ApplicationException("Failed to start process!");

                var p = process;
                using (token.Register(() => p.Kill())) {
                    var readOutTask = ReadToOutput(process.StandardOutput, ConsoleColor.Gray, token);
                    var readErrorTask = ReadToOutput(process.StandardError, ConsoleColor.DarkYellow, token);

                    await Task.Run(() => p.WaitForExit(), token);
                    await Task.WhenAll(readOutTask, readErrorTask);

                    return new ProcessResult {
                        ExitCode = process.ExitCode,
                        Output = readOutTask.Result,
                        Error = readErrorTask.Result,
                    };
                }
            }
        }

        private async Task<string> ReadToOutput(StreamReader reader, ConsoleColor color, CancellationToken token = default(CancellationToken))
        {
            var builder = new StringBuilder();

            //using (token.Register(() => reader.BaseStream.Close()))
            while (!reader.EndOfStream) {
                token.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();

                builder.AppendLine(line);
                Output?.WriteLine(line, color);
            }

            return builder.ToString();
        }
    }
}
