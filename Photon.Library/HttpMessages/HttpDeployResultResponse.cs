using Newtonsoft.Json;
using Photon.Framework.Tasks;

namespace Photon.Library.HttpMessages
{
    public class HttpDeployResultResponse
    {
        [JsonProperty("result")]
        public TaskResult Result {get; set;}
    }
}
