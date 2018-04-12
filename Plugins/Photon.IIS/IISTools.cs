using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;

namespace Photon.Plugins.IIS
{
    public class IISTools : IDisposable
    {
        public IDomainContext Context {get;}
        public ServerManager Server {get;}

        public ApplicationPoolTools ApplicationPool {get;}
        public WebSiteTools WebSite {get;}
        public WebApplicationTools WebApplication {get;}


        public IISTools(IDomainContext context, string serverName = "localhost")
        {
            this.Context = context;

            Server = ServerManager.OpenRemote(serverName);

            ApplicationPool = new ApplicationPoolTools(context, Server);
            WebSite = new WebSiteTools(context, Server);
            WebApplication = new WebApplicationTools(context, Server);
        }

        public void Dispose()
        {
            Server?.Dispose();
        }
    }
}
