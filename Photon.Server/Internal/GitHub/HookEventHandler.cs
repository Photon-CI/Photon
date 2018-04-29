using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Photon.Server.Internal.GitHub
{
    public class HookEventHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HookEventHandler));

        private static readonly Dictionary<string, Func<dynamic, GithubCommit>> actionMap;


        static HookEventHandler()
        {
            actionMap = new Dictionary<string, Func<dynamic, GithubCommit>> {
                ["create"] = ParseCreateEvent,
                ["pull_request"] = ParsePullRequestEvent,
                ["push"] = ParsePushEvent,
            };
        }

        public static GithubCommit ParseEvent(string eventType, string json)
        {
            if (!actionMap.TryGetValue(eventType, out var eventAction))
                return null;

            dynamic data = JsonConvert.DeserializeObject(json);
            return eventAction.Invoke(data);
        }

        private static GithubCommit ParseCreateEvent(dynamic data)
        {
            switch ((string)data.ref_type) {
                case "repository":
                    Log.Debug("[WEBHOOK] Repository Created.");

                    return new GithubCommit {
                        RepositoryUrl = data.repository.clone_url,
                        Refspec = $"refs/remotes/origin/{data.master_branch}",
                    };
                case "branch":
                    Log.Debug("[WEBHOOK] Branch Created.");

                    return new GithubCommit {
                        RepositoryUrl = data.repository.clone_url,
                        Refspec = $"refs/remotes/origin/{data.@ref}",
                    };
                case "tag":
                    Log.Debug("[WEBHOOK] Tag Created.");

                    return new GithubCommit {
                        RepositoryUrl = data.repository.clone_url,
                        Refspec = $"refs/tags/{data.@ref}",
                    };
                default:
                    return null;
            }
        }

        private static GithubCommit ParsePullRequestEvent(dynamic data)
        {
            switch ((string)data.action) {
                case "opened":
                    Log.Debug("[WEBHOOK] Pull Request Opened.");

                    return new GithubCommit {
                        RepositoryUrl = data.repository.clone_url,
                        Refspec = $"refs/remotes/origin/{data.pull_request.head.@ref}",
                        Sha = data.pull_request.head.sha,
                    };
                default:
                    return null;
            }
        }

        private static GithubCommit ParsePushEvent(dynamic data)
        {
            Log.Debug("[WEBHOOK] Commit Pushed.");

            return new GithubCommit {
                RepositoryUrl = data.repository.clone_url,
                Refspec = data.@ref,
                Sha = data.head,
            };
        }
    }
}
