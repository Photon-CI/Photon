using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Library
{
    public static class ProcessRunner
    {
        public static ProcessResult Run(string workDir, string command)
        {
            SplitCommand(command, out var _file, out var _args);
            return Run(workDir, _file, _args);
        }

        public static ProcessResult Run(string workDir, string filename, string arguments)
        {
            var _file = Path.Combine(workDir, filename);

            // TODO: Not sure about this line...
            //var _path = Path.GetDirectoryName(_file);

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
                var readOutTask = process.StandardOutput.ReadToEndAsync();
                var readErrorTask = process.StandardError.ReadToEndAsync();

                process.WaitForExit();
                Task.WaitAll(readOutTask, readErrorTask);

                return new ProcessResult {
                    ExitCode = process.ExitCode,
                    Output = readOutTask.Result,
                    Error = readErrorTask.Result,
                };
            }
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
