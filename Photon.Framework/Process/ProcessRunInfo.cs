using System;
using System.Collections.Generic;

namespace Photon.Framework.Process
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
}
