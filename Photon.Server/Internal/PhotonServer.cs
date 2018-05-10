using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Variables;
using Photon.Library;
using Photon.Library.Packages;
using Photon.Server.Internal.Projects;
using Photon.Server.Internal.ServerAgents;
using Photon.Server.Internal.ServerConfiguration;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class PhotonServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonServer));
        public static PhotonServer Instance {get;} = new PhotonServer();

        private HttpReceiver receiver;
        private bool isStarted;

        public ProjectManager Projects {get;}
        public ServerSessionManager Sessions {get;}
        public ProjectDataManager ProjectData {get;}
        public ScriptQueue Queue {get;}
        public ProjectPackageManager ProjectPackages {get;}
        public ApplicationPackageManager ApplicationPackages {get;}
        public MessageProcessorRegistry MessageRegistry {get;}
        public VariableSetCollection Variables {get;}

        public ServerConfigurationManager ServerConfiguration {get;}
        public ServerAgentManager Agents {get;}


        public PhotonServer()
        {
            Projects = new ProjectManager();
            Sessions = new ServerSessionManager();
            ProjectData = new ProjectDataManager();
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetCollection();

            ProjectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            ApplicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            Queue = new ScriptQueue {
                MaxDegreeOfParallelism = Configuration.Parallelism,
            };

            ServerConfiguration = new ServerConfigurationManager();
            Agents = new ServerAgentManager();
        }

        public void Dispose()
        {
            if (isStarted) Stop();

            Queue?.Dispose();
            Sessions?.Dispose();
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            if (isStarted) throw new Exception("Server has already been started!");
            isStarted = true;

            // Load existing or default server configuration
            ServerConfiguration.Load();

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);

            // TODO: Cache Project Package Index?
            //ProjectPackages.Initialize();

            Sessions.Start();
            Queue.Start();


            var taskVariables = LoadVariables();
            var taskHttp = Task.Run(() => StartHttpServer());
            var taskAgents = Task.Run(() => Agents.Load());
            var taskProjects = Task.Run(() => Projects.Initialize());
            var taskProjectData = Task.Run(() => ProjectData.Initialize());

            Task.WaitAll(
                taskVariables,
                taskAgents,
                taskProjects,
                taskProjectData,
                taskHttp);
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

        public async Task Shutdown(TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout)) {
                var token = tokenSource.Token;

                token.Register(() => {
                    Queue.Abort();
                    Sessions.Abort();
                });

                await Task.Run(() => {
                    Queue.Stop();
                    Sessions.Stop();
                }, token);
            }
        }

        public void Abort()
        {
            Queue.Abort();
            Sessions.Abort();

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP Receiver!", error);
            }
        }

        private static async Task<string> ReadAsync(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream)) {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task LoadVariables()
        {
            var taskList = new List<Task>();

            var filename = Path.Combine(Configuration.Directory, "variables.json");

            if (File.Exists(filename)) {
                var task = ReadAsync(filename)
                    .ContinueWith(t => Variables.GlobalJson = t.Result);

                taskList.Add(task);
            }

            if (Directory.Exists(Configuration.VariablesDirectory)) {
                var fileEnum = Directory.EnumerateFiles(Configuration.VariablesDirectory, "*.json");

                foreach (var file in fileEnum) {
                    var file_name = Path.GetFileNameWithoutExtension(file) ?? string.Empty;

                    var task = ReadAsync(file)
                        .ContinueWith(t => Variables.JsonList[file_name] = t.Result);

                    taskList.Add(task);
                }
            }

            await Task.WhenAll(taskList);
        }

        private void StartHttpServer()
        {
            var http = ServerConfiguration.Value.Http;

            var context = new HttpReceiverContext {
                //SecurityMgr = new Internal.Security.SecurityManager(),
                ListenerPath = http.Path,
                ContentDirectories = {
                    new ContentDirectory {
                        DirectoryPath = Configuration.HttpContentDirectory,
                        UrlPath = "/Content/",
                    }
                },
            };

            context.Views.AddFolderFromExternal(Configuration.HttpViewDirectory);

            var httpPrefix = $"http://{http.Host}:{http.Port}/";

            if (!string.IsNullOrEmpty(http.Path))
                httpPrefix = NetPath.Combine(httpPrefix, http.Path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(context);
            receiver.Routes.Scan(Assembly.GetExecutingAssembly());
            receiver.AddPrefix(httpPrefix);

            try {
                receiver.Start();

                Log.Info($"HTTP Server listening at {httpPrefix}");
            }
            catch (Exception error) {
                Log.Error("Failed to start HTTP Receiver!", error);
            }
        }
    }
}
