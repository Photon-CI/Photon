using log4net;
using Photon.Agent.Internal.AgentConfiguration;
using Photon.Agent.Internal.Applications;
using Photon.Agent.Internal.Git;
using Photon.Agent.Internal.Http;
using Photon.Agent.Internal.Security;
using Photon.Communication;
using Photon.Framework;
using Photon.Library;
using Photon.Library.Security;
using Photon.Library.Variables;
using System;
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

        internal MessageListener messageListener {get;}
        private bool isStarted;

        public HttpServer Http {get;}
        public string WorkDirectory {get;}
        public MessageProcessorRegistry MessageRegistry {get;}
        public VariableSetDocumentManager Variables {get;}
        public RepositorySourceManager RepositorySources {get;}
        public AgentConfigurationManager AgentConfiguration {get;}
        public UserGroupManager UserMgr {get;}
        public ApplicationManager ApplicationMgr {get;}

        public AgentContext Context {get; set;}


        public PhotonAgent()
        {
            WorkDirectory = Configuration.WorkDirectory;

            Http = new HttpServer(this);
            MessageRegistry = new MessageProcessorRegistry();
            Variables = new VariableSetDocumentManager();
            RepositorySources = new RepositorySourceManager();
            messageListener = new MessageListener(MessageRegistry);
            AgentConfiguration = new AgentConfigurationManager();
            UserMgr = new UserGroupManager();
            ApplicationMgr = new ApplicationManager();

            Context = new AgentContext();

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

            Context?.Dispose();

            Http.Dispose();
        }

        public void Start()
        {
            if (isStarted) throw new Exception("Agent has already been started!");
            isStarted = true;

            Log.Debug("Starting Agent...");

            // Load existing or default agent configuration
            AgentConfiguration.Load();

            UserMgr.Directory = Configuration.Directory;

            SecurityTest.Initialize(UserMgr);
            ApplicationMgr.Initialize();

            if (!IPAddress.TryParse(AgentConfiguration.Value.Tcp.Host, out var _address))
                throw new Exception($"Invalid TCP Host '{AgentConfiguration.Value.Tcp.Host}'!");

            Http.Initialize();

            MessageRegistry.Scan(Assembly.GetExecutingAssembly());
            MessageRegistry.Scan(typeof(ILibraryAssembly).Assembly);
            MessageRegistry.Scan(typeof(IFrameworkAssembly).Assembly);
            messageListener.Listen(_address, AgentConfiguration.Value.Tcp.Port);

            Context.Start();

            var taskVariables = Task.Run(() => Variables.Load(Configuration.VariablesDirectory));
            var taskRepositories = Task.Run(() => RepositorySources.Initialize());
            var taskHttp = Task.Run(() => Http.Start());

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

            Context.Stop();

            Http.Stop();

            Log.Info("Agent stopped.");
        }

        public async Task Shutdown(TimeSpan timeout)
        {
            Log.Debug("Shutdown started...");

            using (var tokenSource = new CancellationTokenSource(timeout)) {
                var token = tokenSource.Token;

                using (token.Register(async () => await Context.Sessions.Abort())) {
                    await Task.Run(() => Context.Sessions.Stop(), token);
                }
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
