using log4net;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Photon.Agent.Internal.Git;
using Photon.Framework.Variables;

namespace Photon.Agent.Internal
{
    internal class PhotonAgent : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonAgent));

        public static PhotonAgent Instance {get;} = new PhotonAgent();

        private readonly MessageListener messageListener;
        private HttpReceiver receiver;
        private bool isStarted;

        public AgentDefinition Definition {get; private set;}
        public string WorkDirectory {get;}
        public AgentSessionManager Sessions {get;}
        public MessageProcessorRegistry MessageRegistry {get;}
        public VariableSetCollection Variables {get;}
        public RepositorySourceManager RepositorySources {get;}


        public PhotonAgent()
        {
            WorkDirectory = Configuration.WorkDirectory;

            Sessions = new AgentSessionManager();
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetCollection();
            RepositorySources = new RepositorySourceManager();
            messageListener = new MessageListener(MessageRegistry);

            RepositorySources.RepositorySourceDirectory = Configuration.RepositoryDirectory;
            messageListener.ThreadException += MessageListener_ThreadException;
        }

        public void Dispose()
        {
            //if (isStarted) Stop();

            messageListener?.Dispose();
            Sessions?.Dispose();
            receiver?.Dispose();
            receiver = null;
        }

        public void Start()
        {
            if (isStarted) throw new Exception("Agent has already been started!");
            isStarted = true;

            LoadVariables();

            // Load existing or default agent configuration
            Definition = ParseAgentDefinition() ?? new AgentDefinition {
                Http = {
                    Host = "localhost",
                    Port = 8082,
                    Path = "/photon/agent",
                },
            };

            if (!IPAddress.TryParse(Definition.Tcp.Host, out var _address))
                throw new Exception($"Invalid TCP Host '{Definition.Tcp.Host}'!");

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);
            messageListener.Listen(_address, Definition.Tcp.Port);

            Sessions.Start();

            StartHttpServer();

            RepositorySources.Initialize();
        }

        public void Stop()
        {
            if (!isStarted) return;
            isStarted = false;

            messageListener?.StopAsync()
                .GetAwaiter().GetResult();

            Sessions?.Stop();
            receiver?.Stop();
        }

        private AgentDefinition ParseAgentDefinition()
        {
            var file = Configuration.AgentFile ?? "agent.json";
            var path = Path.Combine(Configuration.AssemblyPath, file);
            path = Path.GetFullPath(path);

            Log.Debug($"Loading Agent Definition: {path}");

            if (!File.Exists(path)) {
                Log.Warn($"Agent Definition not found! {path}");
                return null;
            }

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                return JsonSettings.Serializer.Deserialize<AgentDefinition>(stream);
            }
        }

        private void LoadVariables()
        {
            var filename = Path.Combine(Configuration.Directory, "variables.json");
            var errorList = new List<Exception>();

            if (File.Exists(filename)) {
                try {
                    Variables.GlobalJson = File.ReadAllText(filename);
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }

            if (Directory.Exists(Configuration.VariablesDirectory)) {
                var fileEnum = Directory.EnumerateFiles(Configuration.VariablesDirectory, "*.json");
                Parallel.ForEach(fileEnum, file => {
                    var file_name = Path.GetFileNameWithoutExtension(file) ?? string.Empty;

                    try {
                        var json = File.ReadAllText(file);
                        Variables.JsonList[file_name] = json;
                    }
                    catch (Exception error) {
                        errorList.Add(error);
                    }
                });
            }

            if (errorList.Any()) throw new AggregateException(errorList);
        }

        private void StartHttpServer()
        {
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

            var assembly = Assembly.GetExecutingAssembly();
            context.Views.AddAllFromAssembly(assembly, "Photon.Agent.Views");

            var httpPrefix = $"http://{Definition.Http.Host}:{Definition.Http.Port}/";

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
        }

        private void MessageListener_ThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled TCP Message Error!", (Exception)e.ExceptionObject);
        }
    }
}
