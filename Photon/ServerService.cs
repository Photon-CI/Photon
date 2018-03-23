using Photon.Server.Internal;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Photon.Server
{
    public partial class ServerService : ServiceBase
    {
        private HttpReceiver receiver;


        public ServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var context = new HttpReceiverContext {
                SecurityMgr = new Internal.Security.SecurityManager(),
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

            var httpUri = new UriBuilder("http", "localhost", Configuration.Settings.Document.HttpPort);

            var httpPrefix = BuildPrefix(httpUri.Uri);

            receiver = new HttpReceiver(context);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            receiver.Start();
        }

        protected override void OnStop()
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
