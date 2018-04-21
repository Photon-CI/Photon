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
                var branch = repo.Branches[refspec];

                if (branch == null) {
                    output.Write("Git Refspec ", ConsoleColor.DarkYellow)
                        .Write(refspec, ConsoleColor.Yellow)
                        .WriteLine(" was not found!", ConsoleColor.DarkYellow);
                    
                    throw new ApplicationException($"Git Refspec '{refspec}' was not found!");
                }

                if (branch.IsRemote) {
                    var local_branch = repo.CreateBranch(branch.FriendlyName, branch.Tip);
                    repo.Branches.Update(local_branch, b => b.TrackedBranch = branch.CanonicalName);

                    var y = new CheckoutOptions {
                        CheckoutModifiers = CheckoutModifiers.Force,
                    };

                    LibGit2Sharp.Commands.Checkout(repo, local_branch, y);
                }
                else {
                    if (!branch.IsCurrentRepositoryHead) {
                        var y = new CheckoutOptions {
                            CheckoutModifiers = CheckoutModifiers.Force,
                        };

                        LibGit2Sharp.Commands.Checkout(repo, branch, y);
                    }
                }

                var sign = new Signature("photon", "photon@localhost.com", DateTimeOffset.Now);

                var z = new PullOptions();

                LibGit2Sharp.Commands.Pull(repo, sign, z);

                output.WriteLine("Current Commit:", ConsoleColor.DarkBlue)
                    .WriteLine($"  {repo.Head.Tip.Sha}", ConsoleColor.Blue)
                    .WriteLine($"  {repo.Head.Tip.Author?.Name}", ConsoleColor.Blue)
                    .WriteLine(repo.Head.Tip.Message, ConsoleColor.Cyan);
            }
        }
    }
}
