using log4net;
using Photon.Agent.Internal.AgentConfiguration;
using Photon.Agent.Internal.Git;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Framework;
using Photon.Library;
using Photon.Library.Variables;
using PiServerLite.Http;
using PiServerLite.Http.Content;
using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Photon.Agent.Internal.Security;
using Photon.Library.HttpSecurity;
using Photon.Library.Security;

namespace Photon.Agent.Internal
{
    internal class PhotonAgent : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PhotonAgent));

        public static PhotonAgent Instance {get;} = new PhotonAgent();

        private readonly MessageListener messageListener;
        private HttpReceiver receiver;
        private bool isStarted;

        public HttpReceiverContext HttpContext {get; private set;}

        public string WorkDirectory {get;}
        public AgentSessionManager Sessions {get;}
        public MessageProcessorRegistry MessageRegistry {get;}
        public VariableSetDocumentManager Variables {get;}
        public RepositorySourceManager RepositorySources {get;}
        public AgentConfigurationManager AgentConfiguration {get;}
        public UserGroupManager UserMgr {get;}


        public PhotonAgent()
        {
            WorkDirectory = Configuration.WorkDirectory;

            Sessions = new AgentSessionManager();
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetDocumentManager();
            RepositorySources = new RepositorySourceManager();
            messageListener = new MessageListener(MessageRegistry);
            AgentConfiguration = new AgentConfigurationManager();
            UserMgr = new UserGroupManager();

            RepositorySources.RepositorySourceDirectory = Configuration.RepositoryDirectory;
            messageListener.ConnectionReceived += MessageListener_ConnectionReceived;
            messageListener.ThreadException += MessageListener_ThreadException;
        }

        public void Dispose()
        {
            //if (isStarted) Stop();

            try {
                messageListener?.Dispose();
            }
            catch (Exception error) {
                Log.Error("Failed to dispose TCP message listener!", error);
            }

            try {
                Sessions?.Dispose();
            }
            catch (Exception error) {
                Log.Error("Failed to dispose session manager!", error);
            }

            try {
                receiver?.Dispose();
                receiver = null;
            }
            catch (Exception error) {
                Log.Error("Failed to dispose HTTP receiver!", error);
            }
        }

        public void Start()
        {
            if (isStarted) throw new Exception("Agent has already been started!");
            isStarted = true;

            Log.Debug("Starting Agent...");

            // Load existing or default agent configuration
            AgentConfiguration.Load();

            SecurityTest.Initialize(UserMgr);

            if (!IPAddress.TryParse(AgentConfiguration.Value.Tcp.Host, out var _address))
                throw new Exception($"Invalid TCP Host '{AgentConfiguration.Value.Tcp.Host}'!");

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);
            messageListener.Listen(_address, AgentConfiguration.Value.Tcp.Port);

            Sessions.Start();

            var taskVariables = Task.Run(() => Variables.Load(Configuration.VariablesDirectory));
            var taskRepositories = Task.Run(() => RepositorySources.Initialize());
            var taskHttp = Task.Run(() => StartHttpServer());

            Task.WaitAll(
                taskVariables,
                taskRepositories,
                taskHttp);

            Log.Info("Agent started.");
        }

        public void Stop()
        {
            Log.Debug("Stopping Agent...");

            //if (!isStarted) return;
            //isStarted = false;

            if (messageListener != null) {
                try {
                    var timeout = TimeSpan.FromSeconds(30);
                    using (var tokenSource = new CancellationTokenSource(timeout)) {
                        messageListener.Stop(tokenSource.Token);
                    }
                }
                catch (Exception error) {
                    Log.Error("Failed to stop TCP message listener!", error);
                }
            }

            try {
                Sessions?.Stop();
            }
            catch (Exception error) {
                Log.Error("Failed to stop session manager!", error);
            }

            try {
                receiver?.Stop();
            }
            catch (Exception error) {
                Log.Error("Failed to stop HTTP receiver!", error);
            }

            Log.Info("Agent stopped.");
        }

        public async Task Shutdown(TimeSpan timeout)
        {
            Log.Debug("Shutdown started...");

            using (var tokenSource = new CancellationTokenSource(timeout)) {
                var token = tokenSource.Token;

                token.Register(() => {
                    Sessions.Abort();
                });

                await Task.Run(() => {
                    Sessions.Stop();
                }, token);
            }
        }

        private void StartHttpServer()
        {
            var enableSecurity = AgentConfiguration.Value.Security?.Enabled ?? false;
            var httpConfig = AgentConfiguration.Value.Http;

            var contentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpContentDirectory,
                UrlPath = "/Content/",
            };

            var sharedContentDir = new ContentDirectory {
                DirectoryPath = Configuration.HttpSharedContentDirectory,
                UrlPath = "/SharedContent/",
            };

            HttpContext = new HttpReceiverContext {
                //SecurityMgr = new AgentHttpSecurity(),
                ListenerPath = httpConfig.Path,
                ContentDirectories = {
                    contentDir,
                    sharedContentDir,
                },
            };

            if (enableSecurity) {
                var auth = new HybridAuthorization {
                    UserMgr = UserMgr,
                };

                HttpContext.SecurityMgr = new AgentHttpSecurity {
                    Authorization = auth,
                };
            }

            HttpContext.Views.AddFolderFromExternal(Configuration.HttpViewDirectory);

            var httpPrefix = $"http://{httpConfig.Host}:{httpConfig.Port}/";

            if (!string.IsNullOrEmpty(httpConfig.Path))
                httpPrefix = NetPath.Combine(httpPrefix, httpConfig.Path);

            if (!httpPrefix.EndsWith("/"))
                httpPrefix += "/";

            receiver = new HttpReceiver(HttpContext);
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

        private void MessageListener_ThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled TCP Message Error!", (Exception)e.ExceptionObject);
        }
    }
}
