using Newtonsoft.Json;

namespace Photon.Framework
{
    public class ContextProjectDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}
    }
}
