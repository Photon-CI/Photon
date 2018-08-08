using Photon.Agent.Internal;
using Photon.Framework;
using PiServerLite.Http.Handlers;
using System;
using System.Runtime.InteropServices;

namespace Photon.Agent.ViewModels
{
    internal class IndexVM : AgentViewModel
    {
        public string AgentName {get; set;}
        public string AgentVersion {get; set;}
        public string AgentHttpUrl {get; set;}
        public string AgentTcpUrl {get; set;}

        public string MachineName {get; set;}
        public string MachineProcessorCount {get; set;}
        public string MachineOsVersion {get; set;}
        public string MachineClrVersion {get; set;}
        public string MachineArchitecture {get; set;}
        public string OsDescription {get; set;}
        public string OsArchitecture {get; set;}
        public string FrameworkDescription {get; set;}


        public IndexVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            AgentName = GetAgentName();
            AgentVersion = Internal.Configuration.Version;
            AgentHttpUrl = GetAgentHttpUrl();
            AgentTcpUrl = GetAgentTcpUrl();

            MachineName = Environment.MachineName;
            MachineProcessorCount = Environment.ProcessorCount.ToString("N0");
            MachineOsVersion = Environment.OSVersion.VersionString;
            MachineClrVersion = Environment.Version.ToString();
            MachineArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

            OsDescription = RuntimeInformation.OSDescription;
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString();
            FrameworkDescription = RuntimeInformation.FrameworkDescription;
        }

        private static string GetAgentName()
        {
            var name = PhotonAgent.Instance.AgentConfiguration.Value.Name;

            return !string.IsNullOrEmpty(name)
                ? name : "Photon Agent";
        }

        private static string GetAgentHttpUrl()
        {
            var http = PhotonAgent.Instance.AgentConfiguration.Value.Http;
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
            var tcp = PhotonAgent.Instance.AgentConfiguration.Value.Tcp;
            var _hostIsWild = tcp.Host == "0.0.0.0";
            var _host = _hostIsWild ? "localhost" : tcp.Host;

            return $"tcp://{_host}:{tcp.Port}/";
        }
    }
}
