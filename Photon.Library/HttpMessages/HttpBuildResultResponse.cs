using Newtonsoft.Json;
using Photon.Framework.Server;

namespace Photon.Library.HttpMessages
{
    public class HttpBuildResultResponse
    {
        [JsonProperty("result")]
        public ScriptResult Result {get; set;}

        [JsonProperty("buildNumber")]
        public int BuildNumber {get; set;}
    }
}
