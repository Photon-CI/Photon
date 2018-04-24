using Newtonsoft.Json;
using Photon.Framework;
using System.Net;
using System.Threading.Tasks;

namespace Photon.Library
{
    public static class DownloadTools
    {
        private const string ServerUrl = "http://photon.null511.info";


        public static async Task<dynamic> GetLatestAgentIndex()
        {
            using (var webClient = new WebClient()) {
                var indexUrl = NetPath.Combine(ServerUrl, "api/agent/index");
                var json = await webClient.DownloadStringTaskAsync(indexUrl);
                return JsonConvert.DeserializeObject(json);
            }
        }

        public static async Task<dynamic> GetLatestServerIndex()
        {
            using (var webClient = new WebClient()) {
                var indexUrl = NetPath.Combine(ServerUrl, "api/server/index");
                var json = await webClient.DownloadStringTaskAsync(indexUrl);
                return JsonConvert.DeserializeObject(json);
            }
        }
    }
}
