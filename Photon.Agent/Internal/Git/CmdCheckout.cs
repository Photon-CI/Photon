﻿using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Process;
using System;
using System.Threading;
using System.Web;

namespace Photon.Agent.Internal.Git
{
    internal class CmdCheckout : ICheckout
    {
        private readonly IDomainContext context;

        public string Exe {get; set;}
        public IWriteBlocks Output {get; set;}
        public RepositorySource Source {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}
        public bool EnableTracing {get; set;}

        public string CommitHash {get; private set;}
        public string CommitAuthor {get; private set;}
        public string CommitMessage {get; private set;}


        public CmdCheckout(IDomainContext context = null)
        {
            this.context = context;

            Exe = "git";
            EnableTracing = false;
            Output = context?.Output;
        }

        public void Checkout(string refspec = "master", CancellationToken token = default(CancellationToken))
        {
            if (!GitIsValidRepo()) {
                GitClone(token);
            }

            GitFetch(token);

            GitCheckout(refspec, token);

            var headCommit = GitGetHeadCommitSha(token);
            var commonCommit = GitGetCommonCommit(refspec, token);

            if (!string.Equals(headCommit, commonCommit))
                GitResetHard(commonCommit, token);

            GitPull(token);

            CommitHash = GitGetHeadCommitSha(token);
            CommitAuthor = GitGetHeadCommitAuthor(token);
            CommitMessage = GitGetHeadCommitMessage(token);
        }

        private bool GitIsValidRepo(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: "rev-parse --is-inside-work-tree",
                token: token);

            return r.ExitCode == 0 && string.Equals(r.Output.Trim(), "true");
        }

        private string GitGetHeadCommitSha(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: "rev-parse HEAD",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to get HEAD commit SHA!");

            return r.Output.Trim();
        }

        private string GitGetHeadCommitAuthor(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: "show -s --format=\"%aN <%aE>\" HEAD",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to get HEAD commit author!");

            return r.Output.Trim();
        }

        private string GitGetHeadCommitMessage(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: "show -s --format=\"%B\" HEAD",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to get HEAD commit author!");

            return r.Output.Trim();
        }

        private string GitGetCommonCommit(string refspec, CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: $"merge-base HEAD origin/{refspec}",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to get common commit between HEAD and remote!");

            return r.Output.Trim();
        }

        private void GitClone(CancellationToken token = default(CancellationToken))
        {
            var result = GitCmd(
                arguments: $"clone -v {CredentialsUrl()} \"{Source.RepositoryPath}\"",
                printArgs: $"clone -v {Source.RepositoryUrl}",
                token: token);

            if (result.ExitCode != 0) throw new Exception("Failed to clone repository!");
        }

        private void GitFetch(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: "fetch -p -P -t",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to fetch remotes!");
        }

        private void GitResetHard(string commit, CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: $"reset --hard {commit}",
                token: token);

            if (r.ExitCode != 0) throw new Exception($"Failed to reset branch to commit '{commit}'!");
        }

        private void GitCheckout(string refspec, CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: $"checkout -f {refspec}",
                token: token);

            if (r.ExitCode != 0) throw new Exception($"Failed to checkout refspec '{refspec}'!");
        }

        private void GitPull(CancellationToken token = default(CancellationToken))
        {
            var r = GitCmd(
                arguments: "pull",
                token: token);

            if (r.ExitCode != 0) throw new Exception("Failed to pull updates from remote!");
        }

        private ProcessResult GitCmd(string arguments, string printArgs = null, CancellationToken token = default(CancellationToken))
        {
            Output.WriteLine($" > git {printArgs ?? arguments}", ConsoleColor.White);

            var runInfo = new ProcessRunInfo {
                Filename = Exe,
                Arguments = arguments,
                WorkingDirectory = Source.RepositoryPath,
                EchoCommand = false,
            };

            if (EnableTracing)
                runInfo.EnvironmentVariables["GIT_TRACE"] = "1";

            return new ProcessRunner(context) {
                Output = Output,
            }.Run(runInfo, token);
        }

        private string CredentialsUrl()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                return Source.RepositoryUrl;

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
