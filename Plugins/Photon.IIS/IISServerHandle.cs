using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Plugins.IIS
{
    internal class IISServerHandle : IDisposable
    {
        private readonly string serverName;
        private ServerManager _server;

        public IDomainContext Context {get;}
        public ServerManager Server => GetServer();


        public IISServerHandle(IDomainContext context, string serverName)
        {
            this.Context = context;
            this.serverName = serverName;
        }

        public void Dispose()
        {
            _server?.Dispose();
        }

        public bool CommitChanges()
        {
            _server.CommitChanges();

            return DisposeServer();
        }

        public bool CommitChanges(TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout)) {
                Task.Run(() => _server.CommitChanges(), tokenSource.Token);
            }

            return DisposeServer();
        }

        public async Task<bool> CommitChangesAsync(CancellationToken token)
        {
            await Task.Run(() => _server.CommitChanges(), token);

            return DisposeServer();
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
