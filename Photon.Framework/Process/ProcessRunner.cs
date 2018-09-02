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
    public static class ProcessRunner
    {
        public static SysProcess Start(ProcessRunInfo info)
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

            return SysProcess.Start(startInfo);
        }

        public static ProcessResult Run(ProcessRunInfo info, IWriteAnsi output = null, CancellationToken token = default(CancellationToken))
        {
            using (var process = Start(info)) {
                if (process == null)
                    throw new ApplicationException("Failed to start process!");

                var p = process;
                using (token.Register(() => p.Kill())) {
                    var readOutTask = ReadToOutput(process.StandardOutput, output, ConsoleColor.Gray, token);
                    var readErrorTask = ReadToOutput(process.StandardError, output, ConsoleColor.DarkYellow, token);

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

        public static async Task<ProcessResult> RunAsync(ProcessRunInfo info, IWriteAnsi output = null, CancellationToken token = default(CancellationToken))
        {
            using (var process = Start(info)) {
                if (process == null)
                    throw new ApplicationException("Failed to start process!");

                var p = process;
                using (token.Register(() => p.Kill())) {
                    var readOutTask = ReadToOutput(process.StandardOutput, output, ConsoleColor.Gray, token);
                    var readErrorTask = ReadToOutput(process.StandardError, output, ConsoleColor.DarkYellow, token);

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

        private static async Task<string> ReadToOutput(StreamReader reader, IWriteAnsi output, ConsoleColor color, CancellationToken token = default(CancellationToken))
        {
            var builder = new StringBuilder();

            //using (token.Register(() => reader.BaseStream.Close()))
            while (!reader.EndOfStream) {
                token.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();

                builder.AppendLine(line);
                output?.WriteLine(line, color);
            }

            return builder.ToString();
        }
    }
}
