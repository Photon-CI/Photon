using log4net;
using Newtonsoft.Json;
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
            // Load existing or default agent configuration
            var agent = ParseAgentDefinition() ?? new AgentDefinition {
                Http = {
                    Host = "localhost",
                    Port = 80,
                    Path = "/photon",
                },
            };

            var context = new HttpReceiverContext {
                //SecurityMgr = new Internal.Security.SecurityManager(),
                ListenerPath = agent.Http.Path,
                ContentDirectories = {
                    new ContentDirectory {
                        DirectoryPath = Path.Combine(Configuration.AssemblyPath, "Content"),
                        UrlPath = "/Content/",
                    }
                },
            };

            var viewPath = Path.Combine(Configuration.AssemblyPath, "Views");
            context.Views.AddFolderFromExternal(viewPath);

            var httpPrefix = $"http://+:{agent.Http.Port}/";
            if (!string.IsNullOrEmpty(agent.Http.Path))
                httpPrefix = NetPath.Combine(httpPrefix, agent.Http.Path);

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

        private AgentDefinition ParseAgentDefinition()
        {
            var file = Configuration.DefinitionPath ?? "agent.json";
            var path = Path.Combine(Configuration.AssemblyPath, file);
            path = Path.GetFullPath(path);

            Log.Debug($"Loading Agent Definition: {path}");

            if (!File.Exists(path)) {
                Log.Warn($"Agent Definition not found! {path}");
                return null;
            }

            var serializer = new JsonSerializer();

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader)) {
                return serializer.Deserialize<AgentDefinition>(jsonReader);
            }
        }
    }
}
