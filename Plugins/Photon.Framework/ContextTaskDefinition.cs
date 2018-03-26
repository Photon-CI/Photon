using Newtonsoft.Json;

namespace Photon.Framework
{
    public class ContextTaskDefinition
    {
        [JsonProperty("name")]
        public string Name {get; set;}
    }
}
