using Photon.Framework;
using Photon.Library;
using Photon.Server.Internal;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Photon.Server.ViewModels
{
    internal class IndexVM : ViewModelBase
    {
        public string ServerName {get; set;}
        public string ServerVersion {get; set;}
        public string ServerHttpUrl {get; set;}

        public string MachineName {get; set;}
        public string MachineHost {get; set;}
        public string MachineProcessorCount {get; set;}
        public string MachineOsVersion {get; set;}
        public string MachineClrVersion {get; set;}
        public string MachineArchitecture {get; set;}
        public string OsDescription {get; set;}
        public string OsArchitecture {get; set;}
        public string FrameworkDescription {get; set;}
        public string ProcessArchitecture {get; set;}


        public void Build()
        {
            ServerName = GetServerName();
            ServerVersion = Configuration.Version;
            ServerHttpUrl = GetServerHttpUrl();
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

            MachineName = Environment.MachineName;
            MachineHost = Dns.GetHostName();
            MachineProcessorCount = Environment.ProcessorCount.ToString("N0");
            MachineOsVersion = Environment.OSVersion.VersionString;
            MachineClrVersion = Environment.Version.ToString();
            MachineArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

            OsDescription = RuntimeInformation.OSDescription;
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString();
            FrameworkDescription = RuntimeInformation.FrameworkDescription;
        }

        private static string GetServerName()
        {
            var name = PhotonServer.Instance.ServerConfiguration.Value.Name;

            return !string.IsNullOrEmpty(name)
                ? name : "Photon Agent";
        }

        private static string GetServerHttpUrl()
        {
            var http = PhotonServer.Instance.ServerConfiguration.Value.Http;
            var _port = http.Port != 80 ? $":{http.Port}" : string.Empty;
            var _hostIsWild = http.Host == "*" || http.Host == "+";
            var _host = _hostIsWild ? "localhost" : http.Host;

            var url = $"http://{_host}{_port}/";

            if (!string.IsNullOrEmpty(http.Path))
                url = NetPath.Combine(url, http.Path, string.Empty);

            return url;
        }
    }
}
