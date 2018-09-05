using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;
using Photon.Framework;

namespace Photon.Plugins.IIS
{
    internal class IISServerHandle : IDisposable
    {
        private readonly string serverName;
        private ServerManager _server;

        public IDomainContext Context {get;}
        public TimeSpan CommitTimeout {get; set;}
        public ServerManager Server => GetServer();


        public IISServerHandle(IDomainContext context, string serverName)
        {
            this.Context = context;
            this.serverName = serverName;

            CommitTimeout = TimeSpan.FromSeconds(60);
        }

        public void Dispose()
        {
            _server?.Dispose();
        }

        public bool CommitChanges(CancellationToken token = default(CancellationToken))
        {
            using (var timeoutToken = new TimeoutTokenSource(CommitTimeout, token)) {
                return Task.Run(() => {
                    _server.CommitChanges();
                    return DisposeServer();
                }, timeoutToken.Token)
                    .GetAwaiter().GetResult();
            }
        }

        public async Task<bool> CommitChangesAsync(CancellationToken token = default(CancellationToken))
        {
            using (var timeoutToken = new TimeoutTokenSource(CommitTimeout, token)) {
                return await Task.Run(() => {
                    _server.CommitChanges();
                    return DisposeServer();
                }, timeoutToken.Token);
            }
        }

        private ServerManager GetServer()
        {
            if (_server != null) return _server;

            _server = ServerManager.OpenRemote(serverName);
            return _server;
        }

        private bool DisposeServer()
        {
            try {
                _server.Dispose();
                return true;
            }
            catch {
                return false;
            }
            finally {
                _server = null;
            }
        }
    }
}
