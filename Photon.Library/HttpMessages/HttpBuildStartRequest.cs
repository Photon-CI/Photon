using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Photon.Library.HttpMessages
{
    public class HttpBuildStartRequest
    {
        [JsonProperty("project")]
        public string ProjectId {get; set;}

        [JsonProperty("task")]
        public string TaskName {get; set;}

        [JsonProperty("refspec")]
        public string GitRefspec {get; set;}

        [JsonProperty("mode")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AgentStartModes Mode {get; set;}

        [JsonProperty("roles")]
        public string[] Roles {get; set;}


        public HttpBuildStartRequest()
        {
            Mode = AgentStartModes.Any;
        }
    }

    public enum AgentStartModes
    {
        Any,
        All,
    }
}
