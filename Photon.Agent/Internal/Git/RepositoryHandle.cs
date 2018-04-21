using LibGit2Sharp;
using Photon.Agent.Internal.Session;
using System;

namespace Photon.Agent.Internal.Git
{
    internal class RepositoryHandle : IDisposable
    {
        private readonly Action disposeAction;

        public RepositorySource Source {get;}


        public RepositoryHandle(RepositorySource source, Action disposeAction)
        {
            this.Source = source;
            this.disposeAction = disposeAction;
        }

        public void Dispose()
        {
            disposeAction?.Invoke();
        }

        public void Checkout(SessionOutput output, string refspec = "origin/master")
        {
            if (!Repository.IsValid(Source.RepositoryPath)) {
                output.WriteLine("Cloning Repository...", ConsoleColor.DarkCyan);

                Repository.Clone(Source.RepositoryUrl, Source.RepositoryPath);
            }

            output.WriteLine($"Checking out commit '{refspec}'...", ConsoleColor.DarkCyan);

            using (var repo = new Repository(Source.RepositoryPath)) {
                LibGit2Sharp.Commands.Checkout(repo, refspec);

                //var originMaster = repo.Branches[refspec];
                //repo.Reset(ResetMode.Hard, originMaster.Tip);

                output.WriteLine("Current Commit:", ConsoleColor.DarkBlue)
                    .WriteLine($"  {repo.Head.Tip.Sha}", ConsoleColor.Blue)
                    .WriteLine($"  {repo.Head.Tip.Author?.Name}", ConsoleColor.Blue)
                    .WriteLine(repo.Head.Tip.Message, ConsoleColor.Cyan);
            }
        }
    }
}
