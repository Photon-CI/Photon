using Photon.Framework;
using Photon.Library;
using Photon.Server.Internal;

namespace Photon.Server.ViewModels
{
    internal class IndexVM : ViewModelBase
    {
        public string Name {get; set;}
        public string Version {get; set;}
        public string Url {get; set;}


        public override void Build()
        {
            Name = PhotonServer.Instance.ServerConfiguration.Value.Name;
            Version = Configuration.Version;

            if (string.IsNullOrEmpty(Name))
                Name = "Photon Server";

            var http = PhotonServer.Instance.ServerConfiguration.Value.Http;
            var _port = http.Port != 80 ? $":{http.Port}" : string.Empty;
            var _hostIsWild = http.Host == "*" || http.Host == "+";
            var _host = _hostIsWild ? "localhost" : http.Host;

            Url = $"http://{_host}{_port}/";

            if (!string.IsNullOrEmpty(http.Path))
                Url = NetPath.Combine(Url, http.Path, string.Empty);
        }
    }
}
