using log4net;
using Photon.Framework;
using Photon.Library.Http.Security;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.Reflection;

namespace Photon.Agent.Internal.Http
{
    internal class HttpServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpServer));

        private readonly PhotonAgent agent;
        private HttpReceiver receiver;

        public HttpReceiverContext Context {get;}
        public HttpSecurityManager Security {get;}
        public HybridAuthorization Authorization {get;}


        public HttpServer(PhotonAgent agent)
        {
            this.agent = agent;

            Authorization = new HybridAuthorization();

            Security = new HttpSecurityManager {
                Authorization = Authorization,
                CookieName = "PHOTON.AGENT.AUTH",
            };

            Context = new HttpReceiverContext {
                SecurityMgr = Security,
            };
        }

        public void Initialize()
        {
            var contentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpContentDirectory,
                UrlPath = "/Content/",
            };
            Context.ContentDirectories.Add(contentDir);

            var sharedContentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpSharedContentDirectory,
                UrlPath = "/SharedContent/",
            };
            Context.ContentDirectories.Add(sharedContentDir);

            Context.Views.AddFolderFromExternal(Configuration.HttpViewDirectory);
        }

        public void Dispose()
        {
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            var config = agent.AgentConfiguration.Value;

            Context.ListenerPath = config.Http.Path;

            Security.Restricted = config.Security?.Enabled ?? false;

            Authorization.DomainEnabled = config.Security?.DomainEnabled ?? false;
            Authorization.UserMgr = agent.UserMgr;

            StartReceiver();
        }

        public void Stop()
        {
            StopReceiver();
        }

        private void StartReceiver()
        {
            var httpConfig = agent.AgentConfiguration.Value.Http;

            var httpPrefix = $"http://{httpConfig.Host}:{httpConfig.Port}/";

            if (!string.IsNullOrEmpty(httpConfig.Path))
                httpPrefix = NetPath.Combine(httpPrefix, httpConfig.Path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(Context);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            try {
                receiver.Start();

                Log.Debug($"HTTP Server listening at {httpPrefix}");
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }
        }

        private void StopReceiver()
        {
            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }
        }
    }
}
