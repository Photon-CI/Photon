using Photon.Agent.Internal;
using Photon.Framework.Extensions;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;

namespace Photon.Agent.ViewModels.Configuration
{
    internal class ConfigurationIndexVM : AgentViewModel
    {
        public string HttpHost {get; set;}
        public string HttpPath {get; set;}
        public int HttpPort {get; set;}
        public string TcpHost {get; set;}
        public int TcpPort {get; set;}


        public ConfigurationIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Agent Configuration";
        }

        public void Load()
        {
            var config = PhotonAgent.Instance.AgentConfiguration.Value;

            HttpHost = config.Http.Host;
            HttpPath = config.Http.Path;
            HttpPort = config.Http.Port;
            TcpHost = config.Tcp.Host;
            TcpPort = config.Tcp.Port;
        }

        public void Restore(NameValueCollection form)
        {
            HttpHost = form.Get(nameof(HttpHost));
            HttpPath = form.Get(nameof(HttpPath));
            HttpPort = form.Get(nameof(HttpPort)).To<int>();
            TcpHost = form.Get(nameof(TcpHost));
            TcpPort = form.Get(nameof(TcpPort)).To<int>();
        }

        public void Save()
        {
            var configMgr = PhotonAgent.Instance.AgentConfiguration;

            var hasHttpChanges = false;
            var hasTcpChanges = false;

            if (!string.Equals(configMgr.Value.Http.Host, HttpHost)) {
                configMgr.Value.Http.Host = HttpHost;
                hasHttpChanges = true;
            }

            if (!string.Equals(configMgr.Value.Http.Path, HttpPath)) {
                configMgr.Value.Http.Path = HttpPath;
                hasHttpChanges = true;
            }

            if (configMgr.Value.Http.Port != HttpPort) {
                configMgr.Value.Http.Port = HttpPort;
                hasHttpChanges = true;
            }

            if (!string.Equals(configMgr.Value.Tcp.Host, TcpHost)) {
                configMgr.Value.Tcp.Host = TcpHost;
                hasTcpChanges = true;
            }

            if (configMgr.Value.Tcp.Port != TcpPort) {
                configMgr.Value.Tcp.Port = TcpPort;
                hasTcpChanges = true;
            }

            try {
                if (hasHttpChanges || hasTcpChanges)
                    configMgr.Save();
            }
            catch (Exception error) {
                Errors.Add(new ApplicationException("Failed to save configuration values!", error));
            }

            if (hasHttpChanges)
                RestartHttpServer();

            if (hasTcpChanges)
                RestartTcpServer();
        }

        private void RestartHttpServer()
        {
            try {
                PhotonAgent.Instance.Http.Stop();
                PhotonAgent.Instance.Http.Start();
            }
            catch (Exception error) {
                Errors.Add(new ApplicationException("Failed to restart HTTP server!", error));
            }
        }

        private void RestartTcpServer()
        {
            try {
                var tcpConfig = PhotonAgent.Instance.AgentConfiguration.Value.Tcp;

                if (!IPAddress.TryParse(tcpConfig.Host, out var _address))
                    throw new Exception($"Invalid TCP Host '{tcpConfig.Host}'!");

                using (var tokenSource = new CancellationTokenSource(20_000)) {
                    PhotonAgent.Instance.messageListener.Stop(tokenSource.Token);
                }

                PhotonAgent.Instance.messageListener.Listen(_address, tcpConfig.Port);
            }
            catch (Exception error) {
                Errors.Add(new ApplicationException("Failed to restart HTTP server!", error));
            }
        }
    }
}
