using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;

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

        public void CommitChanges()
        {
            _server.CommitChanges();
            _server.Dispose();
            _server = null;
        }

        private ServerManager GetServer()
        {
            if (_server != null) return _server;

            _server = ServerManager.OpenRemote(serverName);
            return _server;
        }
    }
}
