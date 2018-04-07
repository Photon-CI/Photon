using log4net;
using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Server.Internal.Projects;
using Photon.Server.Internal.Sessions;
using Photon.Server.Internal.Tasks;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.IO;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonServer));
        public static PhotonServer Instance {get;} = new PhotonServer();

        private HttpReceiver receiver;
        private bool isStarted;

        public ServerDefinition Definition {get; private set;}
        public ProjectManager Projects {get;}
        public ServerSessionManager Sessions {get;}
        public ServerTaskRunnerManager TaskRunners {get;}
        public ProjectDataManager ProjectData {get;}
        public ScriptQueue Queue {get;}
        public string WorkPath {get;}


        public PhotonServer()
        {
            Projects = new ProjectManager();
            Sessions = new ServerSessionManager();
            TaskRunners = new ServerTaskRunnerManager();
            ProjectData = new ProjectDataManager();
            Queue = new ScriptQueue();

            WorkPath = Configuration.WorkDirectory;
        }

        public void Dispose()
        {
            if (isStarted) Stop();

            TaskRunners?.Dispose();
            Sessions?.Dispose();
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            isStarted = true;

            // Load existing or default server configuration
            Definition = ParseServerDefinition() ?? new ServerDefinition {
                Http = {
                    Host = "localhost",
                    Port = 8088,
                    Path = "/photon/server",
                },
            };

            var context = new HttpReceiverContext {
                //SecurityMgr = new Internal.Security.SecurityManager(),
                ListenerPath = Definition.Http.Path,
                ContentDirectories = {
                    new ContentDirectory {
                        DirectoryPath = Path.Combine(Configuration.AssemblyPath, "Content"),
                        UrlPath = "/Content/",
                    }
                },
            };

            var viewPath = Path.Combine(Configuration.AssemblyPath, "Views");
            context.Views.AddFolderFromExternal(viewPath);

            var httpPrefix = $"http://+:{Definition.Http.Port}/";

            if (!string.IsNullOrEmpty(Definition.Http.Path))
                httpPrefix = NetPath.Combine(httpPrefix, Definition.Http.Path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(context);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            try {
                receiver.Start();

                Log.Info($"HTTP Server listening at http://{httpPrefix}");
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }

            Projects.Initialize();
            ProjectData.Initialize();

            TaskRunners.Start();
            Sessions.Start();
            Queue.Start();
        }

        public void Stop()
        {
            Queue.Stop();
            Sessions.Stop();
            TaskRunners.Stop();

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }
        }

        //public IEnumerable<AgentDefinition> GetAgents(params string[] roles)
        //{
        //    foreach (var agent in Definition.Agents) {
        //        if (agent.MatchesRoles(roles))
        //            yield return new BuildScriptAgent(agent);
        //    }
        //}

        private ServerDefinition ParseServerDefinition()
        {
            var file = Configuration.ServerFile ?? "server.json";
            var path = Path.Combine(Configuration.AssemblyPath, file);
            path = Path.GetFullPath(path);

            Log.Debug($"Loading Server Definition: {path}");

            if (!File.Exists(path)) {
                Log.Warn($"Server Definition not found! {path}");
                return null;
            }

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<ServerDefinition>(stream);
            }
        }
    }
}
