using Photon.Framework.Server;
using System;

namespace Photon.Server.Internal
{
    [Serializable]
    public class ServerBuildContext : ServerContextBase, IServerBuildContext
    {
        public string PreBuild {get; set;}
        public string GitRefspec {get; set;}
        public string TaskName {get; set;}
        public int BuildNumber {get; set;}
    }
}
