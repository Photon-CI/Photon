using Newtonsoft.Json;
using System;

namespace Photon.Framework.Projects
{
    [Serializable]
    public class ProjectFileSystemSource : IProjectSource
    {
        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("path")]
        public string Path {get; set;}
    }
}
