using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Framework
{
    public static class ProcessRunner
    {
        public static Process Start(string workDir, string filename, string arguments)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));

            var parts = filename.Split(Path.DirectorySeparatorChar);
            var firstPart = parts.FirstOrDefault();

            if (firstPart == "." || firstPart == "..") {
                filename = Path.Combine(workDir, filename);
                filename = Path.GetFullPath(filename);
            }

            var startInfo = new ProcessStartInfo {
                FileName = filename,
                Arguments = arguments,
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            return Process.Start(startInfo);
        }

        public static Process Start(string workDir, string command)
        {
            SplitCommand(command, out var _file, out var _args);
            return Start(workDir, _file, _args);
        }

        public static ProcessResult Run(string workDir, string filename, string arguments, IWriteAnsi output = null)
        {
            using (var process = Start(workDir, filename, arguments)) {
                if (process == null)
                    throw new ApplicationException("Failed to start process!");

                var readOutTask = ReadToOutput(process.StandardOutput, output, ConsoleColor.Gray);
                var readErrorTask = ReadToOutput(process.StandardError, output, ConsoleColor.DarkYellow);

                process.WaitForExit();
                Task.WaitAll(readOutTask, readErrorTask);

                return new ProcessResult {
                    ExitCode = process.ExitCode,
                    Output = readOutTask.Result,
                    Error = readErrorTask.Result,
                };
            }
        }

        public static ProcessResult Run(string workDir, string command, IWriteAnsi output = null)
        {
            SplitCommand(command, out var _file, out var _args);
            return Run(workDir, _file, _args, output);
        }

        private static async Task<string> ReadToOutput(StreamReader reader, IWriteAnsi output, ConsoleColor color)
        {
            var builder = new StringBuilder();

            while (!reader.EndOfStream) {
                var line = await reader.ReadLineAsync();

                builder.AppendLine(line);
                output?.WriteLine(line, color);
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
