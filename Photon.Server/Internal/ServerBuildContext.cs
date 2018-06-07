using Photon.Framework.Server;
using Photon.Library.GitHub;
using System;

namespace Photon.Server.Internal
{
    [Serializable]
    public class ServerBuildContext : ServerContextBase, IServerBuildContext
    {
        public uint BuildNumber {get; set;}
        public string PreBuild {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public GithubCommit Commit {get; set;}
    }
}
