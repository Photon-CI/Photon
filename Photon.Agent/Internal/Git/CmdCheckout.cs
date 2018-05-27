using Photon.Agent.Internal.Session;
using Photon.Framework;
using System;

namespace Photon.Agent.Internal.Git
{
    internal class CmdCheckout : ICheckout
    {
        public string Exe {get; set;}
        public SessionOutput Output {get; set;}
        public RepositorySource Source {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}


        public CmdCheckout()
        {
            Exe = "git";
        }

        public void Checkout(string refspec = "master")
        {
            if (!IsValidRepo()) {
                CloneRepo();
            }

            // TODO: Fetch

            // TODO: Checkout

            // TODO: Pull
        }

        private bool IsValidRepo()
        {
            var r = GitCmd(Source.RepositoryPath, "rev-parse --is-inside-work-tree");
            if (r.ExitCode != 0) throw new Exception("Failed to get repo isValid!");
            return string.Equals(r.Output, "true");
        }

        private void CloneRepo()
        {
            var r = GitCmd(Source.RepositoryPath, $"clone {Source.RepositoryUrl}");
            if (r.ExitCode != 0) throw new Exception("Failed to clone repo!");
        }

        private ProcessResult GitCmd(string root, string arguments)
        {
            return ProcessRunner.Run(root, Exe, arguments, Output.Writer);
        }
    }
}
