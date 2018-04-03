using log4net;
using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using Photon.Server.Internal.Project;
using Photon.Server.Internal.Projects;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IServer, IDisposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(PhotonServer));
        public static PhotonServer Instance {get;} = new PhotonServer();

        private HttpReceiver receiver;
        private bool isStarted;

        public string WorkDirectory {get;}
        public ProjectManager Projects {get;}
        public ServerSessionManager Sessions {get;}
        public ScriptQueue Queue {get;}
        public ServerDefinition Definition {get; private set;}
        public ProjectDataManager ProjectData {get;}


        public PhotonServer()
        {
            Projects = new ProjectManager();
            Sessions = new ServerSessionManager();
            Queue = new ScriptQueue();
            ProjectData = new ProjectDataManager();

            WorkDirectory = Configuration.WorkDirectory;
        }

        public void Dispose()
        {
            if (isStarted) Stop();

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
                    Port = 80,
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
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }

            //LoadAllProjectDefinitions();
            Projects.Initialize();
            ProjectData.Initialize(Configuration.ApplicationDataDirectory);

            Sessions.Start();
            Queue.Start();
        }

        public void Stop()
        {
            Queue.Stop();
            Sessions.Stop();

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }
        }

        public IEnumerable<ScriptAgent> GetAgents(params string[] roles)
        {
            foreach (var agent in Definition.Agents) {
                if (agent.MatchesRoles(roles))
                    yield return new ScriptAgent(agent);
            }
        }

        //public ProjectDefinition FindProject(string projectId)
        //{
        //    return Projects?.FirstOrDefault(x => string.Equals(x.Id, projectId, StringComparison.OrdinalIgnoreCase));
        //}

        private ServerDefinition ParseServerDefinition()
        {
            var file = Configuration.DefinitionPath ?? "server.json";
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

        //private void LoadAllProjectDefinitions()
        //{
        //    var dir = Configuration.ProjectDirectory ?? "Projects";
        //    var path = Path.Combine(Configuration.AssemblyPath, dir);
        //    path = Path.GetFullPath(path);

        //    Log.Debug($"Loading Projects Definition from: {path}");

        //    if (!Directory.Exists(path)) {
        //        Log.Warn($"Project Directory not found! {path}");
        //        return;
        //    }

        //    var serializer = new JsonSerializer();
        //    foreach (var file in Directory.EnumerateFiles(path, "*.json")) {
        //        try {
        //            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        //                var project = serializer.Deserialize<ProjectDefinition>(stream);
        //                Projects.Add(project);
        //            }
        //        }
        //        catch (Exception error) {
        //            Log.Error($"Failed to load Project '{file}'!", error);
        //        }
        //    }
        //}
    }
}
