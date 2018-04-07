using Photon.Framework.Sessions;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Library
{
    public static class ProcessRunner
    {
        public static ProcessResult Run(string workDir, string command, ISessionOutput output)
        {
            SplitCommand(command, out var _file, out var _args);
            return Run(workDir, _file, _args, output);
        }

        public static ProcessResult Run(string workDir, string filename, string arguments, ISessionOutput output)
        {
            var _file = Path.Combine(workDir, filename);

            var startInfo = new ProcessStartInfo {
                FileName = _file,
                Arguments = arguments,
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using (var process = Process.Start(startInfo)) {
                if (process == null)
                    throw new ApplicationException("Failed to start process!");

                var readOutTask = ReadToOutput(process.StandardOutput, output);
                var readErrorTask = ReadToOutput(process.StandardError, output);

                process.WaitForExit();
                Task.WaitAll(readOutTask, readErrorTask);

                return new ProcessResult {
                    ExitCode = process.ExitCode,
                    Output = readOutTask.Result,
                    Error = readErrorTask.Result,
                };
            }
        }

        private static async Task<string> ReadToOutput(StreamReader reader, ISessionOutput output)
        {
            var builder = new StringBuilder();

            while (!reader.EndOfStream) {
                var line = await reader.ReadLineAsync();

                builder.AppendLine(line);
                output.WriteLine(line);
            }

            return builder.ToString();
        }

        private static void SplitCommand(string command, out string exe, out string args)
        {
            if (string.IsNullOrEmpty(command)) {
                exe = command;
                args = string.Empty;
                return;
            }

            var firstChar = command[0];

            if (firstChar == '\"') {
                var i = command.IndexOf('\"', 1);
                exe = i >= 0 ? command.Substring(1, i) : command;
                args = i >= 0 ? command.Substring(i + 1) : string.Empty;
            }
            else if (firstChar == '\'') {
                var i = command.IndexOf('\'', 1);
                exe = i >= 0 ? command.Substring(1, i) : command;
                args = i >= 0 ? command.Substring(i + 1) : string.Empty;
            }
            else {
                var i = command.IndexOf(' ');
                exe = i >= 0 ? command.Substring(0, i) : command;
                args = i >= 0 ? command.Substring(i + 1) : string.Empty;
            }
        }
    }
}
