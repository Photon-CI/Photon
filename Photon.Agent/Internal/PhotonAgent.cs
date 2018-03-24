using log4net;
using Photon.Library;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Agent.Internal
{
    internal class PhotonAgent : IDisposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(PhotonAgent));

        private HttpReceiver receiver;


        public PhotonAgent()
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
