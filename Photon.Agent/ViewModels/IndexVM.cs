﻿using Photon.Agent.Internal;
using Photon.Framework;
using Photon.Library;

namespace Photon.Agent.ViewModels
{
    internal class IndexVM : ViewModelBase
    {
        public string AgentName {get; set;}
        public string AgentVersion {get; set;}
        public string AgentHttpUrl {get; set;}
        public string AgentTcpUrl {get; set;}


        public void Build()
        {
            AgentName = GetAgentName();
            AgentVersion = Configuration.Version;
            AgentHttpUrl = GetAgentHttpUrl();
            AgentTcpUrl = GetAgentTcpUrl();
        }

        private static string GetAgentName()
        {
            var name = PhotonAgent.Instance.Definition.Name;

            return !string.IsNullOrEmpty(name)
                ? name : "Photon Agent";
        }

        private static string GetAgentHttpUrl()
        {
            var http = PhotonAgent.Instance.Definition.Http;
            var _port = http.Port != 80 ? $":{http.Port}" : string.Empty;
            var _hostIsWild = http.Host == "*" || http.Host == "+";
            var _host = _hostIsWild ? "localhost" : http.Host;

            var url = $"http://{_host}{_port}/";

            if (!string.IsNullOrEmpty(http.Path))
                url = NetPath.Combine(url, http.Path, string.Empty);

            return url;
        }

        private static string GetAgentTcpUrl()
        {
            var tcp = PhotonAgent.Instance.Definition.Tcp;
            var _hostIsWild = tcp.Host == "0.0.0.0";
            var _host = _hostIsWild ? "localhost" : tcp.Host;

            return $"tcp://{_host}:{tcp.Port}/";
        }
    }
}
