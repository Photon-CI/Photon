using Photon.Framework;
using Photon.Framework.Server;
using System;

namespace Photon.Agent.Internal.Git
{
    internal class CmdCheckout : ICheckout
    {
        public string Exe {get; set;}
        public ScriptOutput Output {get; set;}
        public RepositorySource Source {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}


        public CmdCheckout()
        {
            Exe = "git";
        }

        public void Checkout(string refspec = "master")
        {
            if (!GitIsValidRepo()) {
                GitClone();
            }

            Gitfetch();

            GitCheckout(refspec);

            GitPull();
        }

        private bool GitIsValidRepo()
        {
            var r = GitCmd(Source.RepositoryPath, "rev-parse --is-inside-work-tree");
            return r.ExitCode == 0 && string.Equals(r.Output.Trim(), "true");
        }

        private void GitClone()
        {
            var r = GitCmd(Source.RepositoryPath, $"clone \"{Source.RepositoryUrl}\" \"{Source.RepositoryPath}\"");
            if (r.ExitCode != 0) throw new Exception("Failed to clone repository!");
        }

        private void Gitfetch()
        {
            var r = GitCmd(Source.RepositoryPath, "fetch -p -t");
            if (r.ExitCode != 0) throw new Exception("Failed to fetch remotes!");
        }

        private void GitCheckout(string refspec)
        {
            var r = GitCmd(Source.RepositoryPath, $"checkout -f {refspec}");
            if (r.ExitCode != 0) throw new Exception($"Failed to checkout refspec '{refspec}'!");
        }

        private void GitPull()
        {
            var r = GitCmd(Source.RepositoryPath, "pull");
            if (r.ExitCode != 0) throw new Exception("Failed to pull updates from remote!");
        }

        private ProcessResult GitCmd(string root, string arguments)
        {
            Output.WriteLine($" > git {arguments}", ConsoleColor.White);

            var runInfo = new ProcessRunInfo {
                Filename = Exe,
                Arguments = arguments,
                WorkingDirectory = root,
            };

            return ProcessRunner.Run(runInfo, Output);
        }
    }
}
