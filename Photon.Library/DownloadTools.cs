using Newtonsoft.Json;
using Photon.Framework;
using Photon.Library.HttpMessages;
using System.Net;
using System.Threading.Tasks;

namespace Photon.Library
{
    public static class DownloadTools
    {
        private const string ServerUrl = "http://photon.null511.info";


        public static async Task<HttpPackageIndex> GetLatestAgentIndex()
        {
            using (var webClient = new WebClient()) {
                var indexUrl = NetPath.Combine(ServerUrl, "api/agent/index");
                var json = await webClient.DownloadStringTaskAsync(indexUrl);
                return JsonConvert.DeserializeObject<HttpPackageIndex>(json);
            }
        }

        public static async Task<HttpPackageIndex> GetLatestServerIndex()
        {
            using (var webClient = new WebClient()) {
                var indexUrl = NetPath.Combine(ServerUrl, "api/server/index");
                var json = await webClient.DownloadStringTaskAsync(indexUrl);
                return JsonConvert.DeserializeObject<HttpPackageIndex>(json);
            }
        }

        public static async Task<HttpPackageIndex> GetLatestCliIndex()
        {
            using (var webClient = new WebClient()) {
                var indexUrl = NetPath.Combine(ServerUrl, "api/cli/index");
                var json = await webClient.DownloadStringTaskAsync(indexUrl);
                return JsonConvert.DeserializeObject<HttpPackageIndex>(json);
            }
        }
    }
}
