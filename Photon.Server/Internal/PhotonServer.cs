using log4net;
using Photon.Library;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IDisposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(PhotonServer));

        private HttpReceiver receiver;


        public PhotonServer()
        {
            //
        }

        public void Dispose()
        {
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            var context = new HttpReceiverContext {
                //SecurityMgr = new Internal.Security.SecurityManager(),
                ListenerPath = Configuration.HttpPath,
                ContentDirectories = {
                    new ContentDirectory {
                        DirectoryPath = Path.Combine(Configuration.AssemblyPath, "Content"),
                        UrlPath = "/Content/",
                    }
                },
            };

            var viewPath = Path.Combine(Configuration.AssemblyPath, "Views");
            context.Views.AddFolderFromExternal(viewPath);

            var httpUri = new UriBuilder("http", "localhost", Configuration.HttpPort);

            var port = Configuration.HttpPort;
            var path = Configuration.HttpPath;

            var httpPrefix = $"http://+:{port}/";
            if (!string.IsNullOrEmpty(path))
                httpPrefix = NetPath.Combine(httpPrefix, path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(context);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            try {
                receiver.Start();
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }
        }

        public void Stop()
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
