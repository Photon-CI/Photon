using LibGit2Sharp;
using Photon.Agent.Internal.Session;
using System;

namespace Photon.Agent.Internal.Git
{
    internal class LibCheckout : ICheckout
    {
        public SessionOutput Output {get; set;}
        public RepositorySource Source {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}


        public void Checkout(string refspec = "master")
        {
            var checkoutOptions = new CheckoutOptions {
                CheckoutModifiers = CheckoutModifiers.Force,
            };

            // Clone repository if it does not exist
            if (!Repository.IsValid(Source.RepositoryPath)) {
                Output.WriteLine("Cloning Repository...", ConsoleColor.DarkCyan);

                var cloneOptions = new CloneOptions();
                cloneOptions.CredentialsProvider += CredentialsProvider;

                Repository.Clone(Source.RepositoryUrl, Source.RepositoryPath, cloneOptions);
            }

            using (var repo = new Repository(Source.RepositoryPath)) {
                // Fetch all updated refspecs and tags
                Output.WriteLine("Fetching updated refs...", ConsoleColor.DarkCyan);

                var fetchSpec = new[] {"+refs/heads/*:refs/remotes/origin/*"};
                var fetchOptions = new FetchOptions {
                    TagFetchMode = TagFetchMode.All,
                };

                fetchOptions.CredentialsProvider += CredentialsProvider;

                LibGit2Sharp.Commands.Fetch(repo, "origin", fetchSpec, fetchOptions, null);

                // Find local and remote branches
                var remoteBranchName = $"refs/remotes/origin/{refspec}";
                var remoteBranch = repo.Branches[remoteBranchName];

                var localBranchName = $"refs/heads/origin/{refspec}";
                var localBranch = repo.Branches[localBranchName];

                if (remoteBranch == null) {
                    Output.Write("Git Refspec ", ConsoleColor.DarkYellow)
                        .Write(refspec, ConsoleColor.Yellow)
                        .WriteLine(" was not found!", ConsoleColor.DarkYellow);

                    throw new ApplicationException($"Git Refspec '{refspec}' was not found!");
                }

                if (localBranch != null) {
                    Output.WriteLine($"Found local branch '{localBranch.FriendlyName}'...", ConsoleColor.DarkCyan);

                    // Update tracking branch if not remote branch
                    if (!localBranch.IsTracking || localBranch.TrackedBranch != remoteBranch) {
                        Output.WriteLine("Updating local branch tracking reference...", ConsoleColor.DarkCyan);

                        repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                    }

                    // Checkout local branch if not current
                    if (!localBranch.IsCurrentRepositoryHead) {
                        Output.WriteLine($"Checkout local branch '{localBranch.FriendlyName}'...", ConsoleColor.DarkCyan);

                        LibGit2Sharp.Commands.Checkout(repo, localBranch, checkoutOptions);
                    }

                    // Revert to common ancestor commit if diverged
                    var status = localBranch.TrackingDetails;
                    var aheadCount = status.AheadBy ?? 0;

                    if (aheadCount > 0) {
                        Output.WriteLine($"Local branch '{localBranch.FriendlyName}' has diverged from the remote tracking branch!", ConsoleColor.DarkYellow);

                        var common = status.CommonAncestor;

                        if (common != null) {
                            Output.WriteLine($"Reverting local branch to commit '{common.Sha}'!", ConsoleColor.DarkCyan);

                            repo.Reset(ResetMode.Hard, common, checkoutOptions);
                        }
                    }

                    // Pull latest changes from remote
                    Output.WriteLine("Pull changes from remote...", ConsoleColor.DarkCyan);

                    var sign = new Signature("photon", "photon@localhost.com", DateTimeOffset.Now);

                    var pullOptions = new PullOptions {
                        FetchOptions = fetchOptions,
                    };

                    LibGit2Sharp.Commands.Pull(repo, sign, pullOptions);
                }
                else {
                    // Create local branch tracking remote
                    Output.WriteLine($"No local branch found. Creating local tracking branch '{remoteBranch.FriendlyName}'...", ConsoleColor.DarkCyan);
                    localBranch = repo.CreateBranch(remoteBranch.FriendlyName, remoteBranch.Tip);
                    repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);

                    Output.WriteLine($"Checkout local tracking branch '{localBranch.FriendlyName}'...", ConsoleColor.DarkCyan);

                    LibGit2Sharp.Commands.Checkout(repo, localBranch, checkoutOptions);
                }

                Output.WriteLine("Current Commit:", ConsoleColor.DarkBlue)
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
