using Photon.Framework;
using Photon.Framework.Server;
using System;
using System.Threading;
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

        public void Checkout(string refspec = "master", CancellationToken token = default(CancellationToken))
        {
            if (!GitIsValidRepo()) {
                GitClone(token);
            }

            Gitfetch(token);

            GitCheckout(refspec, token);

            GitPull(token);
        }

        private bool GitIsValidRepo(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: "rev-parse --is-inside-work-tree",
                token: token);

            return r.ExitCode == 0 && string.Equals(r.Output.Trim(), "true");
        }

        private void GitClone(CancellationToken token = default(CancellationToken))
        {
            var result = GitCmd(
                root: Source.RepositoryPath,
                arguments: $"clone -v {CredentialsUrl()} \"{Source.RepositoryPath}\"",
                printArgs: $"clone -v {Source.RepositoryUrl}",
                token: token);

            if (result.ExitCode != 0) throw new Exception("Failed to clone repository!");
        }

        private void Gitfetch(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: "fetch -p -P -t --progress",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to fetch remotes!");
        }

        private void GitCheckout(string refspec, CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: $"checkout -f {refspec}",
                token: token);

            if (r.ExitCode != 0) throw new Exception($"Failed to checkout refspec '{refspec}'!");
        }

        private void GitPull(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                root: Source.RepositoryPath,
                arguments: "pull",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to pull updates from remote!");
        }

        private ProcessResult GitCmd(string root, string arguments, string printArgs = null, CancellationToken token = default(CancellationToken))
        {
            Output.WriteLine($" > git {printArgs ?? arguments}", ConsoleColor.White);

            var runInfo = new ProcessRunInfo {
                Filename = Exe,
                Arguments = arguments,
                WorkingDirectory = root,
                EchoCommand = false,
            };

            return ProcessRunner.Run(runInfo, Output, token);
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
