using Newtonsoft.Json;
using System.Collections.Generic;

namespace Photon.Framework.Packages
{
    public class PackageDefinition
    {
        [JsonProperty("id")]
        public string Id {get; set;}

        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("description")]
        public string Description {get; set;}

        [JsonProperty("include")]
        public List<string> Include {get; set;}

        [JsonProperty("exclude")]
        public List<string> Exclude {get; set;}


        public PackageDefinition()
        {
            Include = new List<string>();
            Exclude = new List<string>();
        }
    }
}
