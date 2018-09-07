using Photon.Framework.Extensions;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Specialized;

namespace Photon.Server.ViewModels.Configuration
{
    internal class ConfigurationIndexVM : ServerViewModel
    {
        public string HttpHost {get; set;}
        public string HttpPath {get; set;}
        public int HttpPort {get; set;}


        public ConfigurationIndexVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server Configuration";
        }

        public void Load()
        {
            var config = PhotonServer.Instance.ServerConfiguration.Value;

            HttpHost = config.Http.Host;
            HttpPath = config.Http.Path;
            HttpPort = config.Http.Port;
        }

        public void Restore(NameValueCollection form)
        {
            HttpHost = form.Get(nameof(HttpHost));
            HttpPath = form.Get(nameof(HttpPath));
            HttpPort = form.Get(nameof(HttpPort)).To<int>();
        }

        public void Save()
        {
            var configMgr = PhotonServer.Instance.ServerConfiguration;

            var hasHttpChanges = false;

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

            try {
                configMgr.Save();
            }
            catch (Exception error) {
                Errors.Add(new ApplicationException("Failed to save configuration values!", error));
            }

            if (hasHttpChanges)
                RestartHttpServer();
        }

        private void RestartHttpServer()
        {
            try {
                PhotonServer.Instance.Http.Stop();
                PhotonServer.Instance.Http.Start();
            }
            catch (Exception error) {
                Errors.Add(new ApplicationException("Failed to restart HTTP server!", error));
            }
        }
    }
}
