using Newtonsoft.Json;
using System;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectDataLastBuild
    {
        [JsonProperty("number")]
        public int Number {get; set;}

        [JsonProperty("time")]
        public DateTime Time {get; set;}
    }
}
