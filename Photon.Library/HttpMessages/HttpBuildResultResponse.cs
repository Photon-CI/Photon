using Newtonsoft.Json;
using Photon.Framework.Tasks;

namespace Photon.Library.HttpMessages
{
    public class HttpBuildResultResponse
    {
        [JsonProperty("buildNumber")]
        public uint BuildNumber {get; set;}

        [JsonProperty("result")]
        public TaskResult Result {get; set;}

        [JsonProperty("projectPackages")]
        public HttpPackageReference[] ProjectPackages {get; set;}

        [JsonProperty("applicationPackages")]
        public HttpPackageReference[] ApplicationPackages {get; set;}
    }
}
