using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework
{
    public class ProcessRunInfo
    {
        public string Filename {get; set;}
        public string Arguments {get; set;}
        public string WorkingDirectory {get; set;}
        public Dictionary<string, string> EnvironmentVariables {get; set;}
        public bool EchoCommand {get; set;}


        public ProcessRunInfo()
        {
            EnvironmentVariables = new Dictionary<string, string>();
        }

        public static ProcessRunInfo FromCommand(string command)
        {
            FromCommandInner(command, out var filename, out var arguments);

            filename = filename?.Trim();
            arguments = arguments?.Trim();

            if (string.IsNullOrEmpty(arguments))
                arguments = null;

            return new ProcessRunInfo {
                Filename = filename,
                Arguments = arguments,
            };
        }

        private static void FromCommandInner(string command, out string filename, out string arguments)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));

            var firstChar = command[0];
            var i = -1;

            if (firstChar == '\"') {
                i = command.IndexOf('\"', 1);
            }
            else if (firstChar == '\'') {
                i = command.IndexOf('\'', 1);
            }

            if (i >= 0) {
                filename = command.Substring(1, i - 1);
                arguments = command.Substring(i + 1);
                return;
            }

            i = command.IndexOf(' ');

            filename = i >= 0 ? command.Substring(0, i) : command;
            arguments = i >= 0 ? command.Substring(i + 1) : null;
        }
    }

    public static class ProcessRunner
    {
        public static Process Start(ProcessRunInfo info)
        {
            if (info.Filename == null) throw new ArgumentNullException(nameof(info.Filename));

            var parts = info.Filename.Split(Path.DirectorySeparatorChar);
            var firstPart = parts.FirstOrDefault();

            if (firstPart == "." || firstPart == "..") {
                info.Filename = Path.Combine(info.WorkingDirectory, info.Filename);
                info.Filename = Path.GetFullPath(info.Filename);
            }

            var startInfo = new ProcessStartInfo {
                FileName = info.Filename,
                Arguments = info.Arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (!string.IsNullOrEmpty(info.WorkingDirectory))
                startInfo.WorkingDirectory = info.WorkingDirectory;

            foreach (var key in info.EnvironmentVariables.Keys)
                startInfo.EnvironmentVariables[key] = info.EnvironmentVariables[key];

            return Process.Start(startInfo);
        }

        public static ProcessResult Run(ProcessRunInfo info, IWriteAnsi output = null, CancellationToken token = default(CancellationToken))
        {
            using (var process = Start(info))
            using (token.Register(() => process.Kill())) {
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
    }
}
