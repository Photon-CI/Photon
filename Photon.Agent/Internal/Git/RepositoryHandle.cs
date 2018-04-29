using LibGit2Sharp;
using Photon.Agent.Internal.Session;
using System;

namespace Photon.Agent.Internal.Git
{
    internal class RepositoryHandle : IDisposable
    {
        private readonly Action disposeAction;

        public RepositorySource Source {get;}
        public string Username {get; set;}
        public string Password {get; set;}


        public RepositoryHandle(RepositorySource source, Action disposeAction)
        {
            this.Source = source;
            this.disposeAction = disposeAction;
        }

        public void Dispose()
        {
            disposeAction?.Invoke();
        }

        public void Checkout(SessionOutput output, string refspec = "master")
        {
            if (!Repository.IsValid(Source.RepositoryPath)) {
                output.WriteLine("Cloning Repository...", ConsoleColor.DarkCyan);

                var cloneOptions = new CloneOptions();
                cloneOptions.CredentialsProvider += CredentialsProvider;

                Repository.Clone(Source.RepositoryUrl, Source.RepositoryPath, cloneOptions);
            }

            output.WriteLine($"Checking out commit '{refspec}'...", ConsoleColor.DarkCyan);

            using (var repo = new Repository(Source.RepositoryPath)) {
                //var branch = LibGit2Sharp.Commands.Checkout(repo, refspec);

                var fetchSpec = new[] {"+refs/heads/*:refs/remotes/origin/*"};
                var fetchOptions = new FetchOptions {
                    TagFetchMode = TagFetchMode.All,
                };

                fetchOptions.CredentialsProvider += CredentialsProvider;

                LibGit2Sharp.Commands.Fetch(repo, "origin", fetchSpec, fetchOptions, null);

                var localBranch = repo.Branches[$"refs/heads/origin/{refspec}"];

                if (localBranch != null) {
                    if (!localBranch.IsCurrentRepositoryHead) {
                        var checkoutOptions = new CheckoutOptions {
                            CheckoutModifiers = CheckoutModifiers.Force,
                        };

                        LibGit2Sharp.Commands.Checkout(repo, localBranch, checkoutOptions);
                    }

                    if (!localBranch.IsTracking) {
                        var remoteBranch = repo.Branches[$"refs/remotes/origin/{refspec}"];

                        if (remoteBranch == null) {
                            output.Write("Git Refspec ", ConsoleColor.DarkYellow)
                                .Write(refspec, ConsoleColor.Yellow)
                                .WriteLine(" was not found!", ConsoleColor.DarkYellow);
                    
                            throw new ApplicationException($"Git Refspec '{refspec}' was not found!");
                        }

                        repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                    }

                    var sign = new Signature("photon", "photon@localhost.com", DateTimeOffset.Now);

                    var pullOptions = new PullOptions {
                        FetchOptions = fetchOptions,
                    };

                    LibGit2Sharp.Commands.Pull(repo, sign, pullOptions);
                }
                else {
                    var remoteBranch = repo.Branches[$"refs/remotes/origin/{refspec}"];

                    if (remoteBranch == null) {
                        output.Write("Git Refspec ", ConsoleColor.DarkYellow)
                            .Write(refspec, ConsoleColor.Yellow)
                            .WriteLine(" was not found!", ConsoleColor.DarkYellow);
                    
                        throw new ApplicationException($"Git Refspec '{refspec}' was not found!");
                    }

                    localBranch = repo.CreateBranch(remoteBranch.FriendlyName, remoteBranch.Tip);
                    repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);

                    var checkoutOptions = new CheckoutOptions {
                        CheckoutModifiers = CheckoutModifiers.Force,
                    };

                    LibGit2Sharp.Commands.Checkout(repo, localBranch, checkoutOptions);
                }

                output.WriteLine("Current Commit:", ConsoleColor.DarkBlue)
                    .WriteLine($"  {repo.Head.Tip.Sha}", ConsoleColor.Blue)
                    .WriteLine($"  {repo.Head.Tip.Author?.Name}", ConsoleColor.Blue)
                    .WriteLine(repo.Head.Tip.Message, ConsoleColor.Cyan);
            }
        }

        private Credentials CredentialsProvider(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials {
                Username = Username,
                Password = Password,
            };
        }
    }
}
