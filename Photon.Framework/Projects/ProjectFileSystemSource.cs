using Newtonsoft.Json;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectFileSystemSource : IProjectSource
    {
        [JsonProperty("path")]
        public string Path {get; set;}
    }
}
