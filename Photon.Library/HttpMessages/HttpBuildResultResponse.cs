using Newtonsoft.Json;
using Photon.Framework.Tasks;

namespace Photon.Library.HttpMessages
{
    public class HttpBuildResultResponse
    {
        [JsonProperty("buildNumber")]
        public int BuildNumber {get; set;}

        [JsonProperty("result")]
        public TaskResult Result {get; set;}

        [JsonProperty("projectPackages")]
        public HttpPackageReference[] ProjectPackages {get; set;}
    }
}
