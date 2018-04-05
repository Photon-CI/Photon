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

        [JsonProperty("assembly")]
        public string Assembly {get; set;}

        [JsonProperty("files")]
        public List<PackageFileDefinition> Files {get; set;}


        public PackageDefinition()
        {
            Files = new List<PackageFileDefinition>();
        }
    }
}
