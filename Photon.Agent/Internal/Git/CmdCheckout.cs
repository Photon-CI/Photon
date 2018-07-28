using Photon.Framework;
using Photon.Framework.Server;
using System;
using System.Web;

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
            var result = GitCmd(
                root: Source.RepositoryPath,
                arguments: $"clone \"{CredentialsUrl()}\" \"{Source.RepositoryPath}\"",
                printArgs: $"clone \"{Source.RepositoryUrl}\"");

            if (result.ExitCode != 0) throw new Exception("Failed to clone repository!");
        }

        private void Gitfetch()
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: $"fetch -p -t \"{CredentialsUrl()}\"",
                printArgs: "fetch -p -t");

            if (r.ExitCode != 0) throw new Exception("Failed to fetch remotes!");
        }

        private void GitCheckout(string refspec)
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: $"checkout -f {refspec} \"{CredentialsUrl()}\"",
                printArgs: $"checkout -f {refspec}");
            if (r.ExitCode != 0) throw new Exception($"Failed to checkout refspec '{refspec}'!");
        }

        private void GitPull()
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: $"pull \"{CredentialsUrl()}\"",
                printArgs: "pull");

            if (r.ExitCode != 0) throw new Exception("Failed to pull updates from remote!");
        }

        private ProcessResult GitCmd(string root, string arguments, string printArgs = null)
        {
            Output.WriteLine($" > git {printArgs ?? arguments}", ConsoleColor.White);

            var runInfo = new ProcessRunInfo {
                Filename = Exe,
                Arguments = arguments,
                WorkingDirectory = root,
            };

            return ProcessRunner.Run(runInfo, Output);
        }

        private string CredentialsUrl()
        {
            var i = Source.RepositoryUrl.IndexOf("://", StringComparison.Ordinal);
            if (i < 0) throw new ApplicationException($"Invalid URL '{Source.RepositoryUrl}'!");

            var pre = Source.RepositoryUrl.Substring(0, i + 3);
            var post = Source.RepositoryUrl.Substring(i + 3);

            var eUser = HttpUtility.UrlEncode(Username);
            var ePass = HttpUtility.UrlEncode(Password);

            return string.Concat(pre, eUser, ":", ePass, "@", post);
        }
    }
}
