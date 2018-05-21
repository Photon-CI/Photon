using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;

namespace Photon.Plugins.IIS
{
    public class IISTools : IDisposable
    {
        private readonly IISServerHandle handle;

        public ServerManager Server => handle.Server;

        public ApplicationPoolTools ApplicationPool {get;}
        public WebSiteTools WebSite {get;}
        public WebApplicationTools WebApplication {get;}


        public IISTools(IDomainContext context, string serverName = "localhost")
        {
            handle = new IISServerHandle(context, serverName);

            ApplicationPool = new ApplicationPoolTools(handle);
            WebSite = new WebSiteTools(handle);
            WebApplication = new WebApplicationTools(handle);
        }

        public void Dispose()
        {
            handle?.Dispose();
        }
    }
}
