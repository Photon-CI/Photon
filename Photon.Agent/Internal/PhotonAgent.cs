using log4net;
using Photon.Agent.Internal.Git;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library;
using Photon.Library.Variables;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        public VariableSetDocumentManager Variables {get;}
        public RepositorySourceManager RepositorySources {get;}


        public PhotonAgent()
        {
            WorkDirectory = Configuration.WorkDirectory;

            Sessions = new AgentSessionManager();
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetDocumentManager();
            RepositorySources = new RepositorySourceManager();
            messageListener = new MessageListener(MessageRegistry);

            RepositorySources.RepositorySourceDirectory = Configuration.RepositoryDirectory;
            messageListener.ConnectionReceived += MessageListener_ConnectionReceived;
            messageListener.ThreadException += MessageListener_ThreadException;
        }

        private void MessageListener_ConnectionReceived(object sender, TcpConnectionReceivedEventArgs e)
        {
            var remote = (IPEndPoint)e.Host.Tcp.Client.RemoteEndPoint;
            Log.Info($"TCP Connection received from '{remote.Address}:{remote.Port}'.");

            e.Accept = false;
            using (var tokenSource = new CancellationTokenSource()) {
                tokenSource.CancelAfter(TimeSpan.FromSeconds(30));

                if (e.Host.GetHandshakeResult(tokenSource.Token).GetAwaiter().GetResult())
                    e.Accept = true;
            }
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

            var taskVariables = Task.Run(() => Variables.Load(Configuration.VariablesDirectory));
            var taskRepositories = Task.Run(() => RepositorySources.Initialize());
            var taskHttp = Task.Run(() => StartHttpServer());

            Task.WaitAll(
                taskVariables,
                taskRepositories,
                taskHttp);
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

        //public async Task Shutdown(TimeSpan timeout)
        //{
        //    using (var tokenSource = new CancellationTokenSource(timeout)) {
        //        var token = tokenSource.Token;

        //        token.Register(() => {
        //            Sessions.Abort();
        //        });

        //        await Task.Run(() => {
        //            Sessions.Stop();
        //        }, token);
        //    }
        //}

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

        private void StartHttpServer()
        {
            var contentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpContentDirectory,
                UrlPath = "/Content/",
            };

            var sharedContentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpSharedContentDirectory,
                UrlPath = "/SharedContent/",
            };

            var context = new HttpReceiverContext {
                //SecurityMgr = new Internal.Security.SecurityManager(),
                ListenerPath = Definition.Http.Path,
                ContentDirectories = {
                    contentDir,
                    sharedContentDir,
                },
            };

            context.Views.AddFolderFromExternal(Configuration.HttpViewDirectory);

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

                Log.Info($"HTTP Server listening at {httpPrefix}");
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
