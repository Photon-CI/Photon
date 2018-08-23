using System;
using System.Net;
using System.Text;

namespace Photon.CLI.Internal.Http
{
    internal static class WebClientFactory
    {
        public static WebClient Create(PhotonServerDefinition server)
        {
            return Create(server, null, null);
        }

        public static WebClient Create(PhotonServerDefinition server, string username, string password)
        {
            var webClient = new WebClient();

            try {
                webClient.BaseAddress = server.Url;

                if (!webClient.BaseAddress.EndsWith("/"))
                    webClient.BaseAddress += "/";

                if (!string.IsNullOrEmpty(username)) {
                    var creds = Encoding.ASCII.GetBytes($"{username}:{password}");
                    var creds64 = Convert.ToBase64String(creds);
                    webClient.Headers[HttpRequestHeader.Authorization] = $"Basic {creds64}";
                }

                return webClient;
            }
            catch {
                webClient.Dispose();
                throw;
            }
        }
    }
}
