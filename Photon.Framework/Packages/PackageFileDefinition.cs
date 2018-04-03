using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageFileDefinition
    {
        [JsonProperty("path")]
        public string Path {get; set;}

        [JsonProperty("destination")]
        public string Destination {get; set;}

        [JsonProperty("exclude")]
        public List<string> Exclude {get; set;}
    }
}
