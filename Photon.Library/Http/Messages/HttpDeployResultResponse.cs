using Newtonsoft.Json;
using Photon.Framework.Tasks;

namespace Photon.Library.Http.Messages
{
    public class HttpDeployResultResponse
    {
        [JsonProperty("result")]
        public TaskResult Result {get; set;}
    }
}
