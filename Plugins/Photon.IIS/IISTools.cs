using Microsoft.Web.Administration;
using System;
using System.Linq;

namespace Photon.Plugins.IIS
{
    public class IISTools : IDisposable
    {
        public ServerManager Server {get;}


        public IISTools()
        {
            Server = new ServerManager();
        }

        public void Dispose()
        {
            Server?.Dispose();
        }

        public void ConfigureAppPool(string appPoolName, Action<ApplicationPool> configureAction)
        {
            var appPool = Server.ApplicationPools.FirstOrDefault(x =>
                string.Equals(x.Name, appPoolName, StringComparison.OrdinalIgnoreCase));

            if (appPool == null) {
                appPool = Server.ApplicationPools.Add(appPoolName);
            }

            configureAction(appPool);

            Server.CommitChanges();
        }

        public void ConfigureWebSite(string websiteName, Action<Site> configureAction)
        {
            var website = Server.Sites.FirstOrDefault(x =>
                string.Equals(x.Name, websiteName, StringComparison.OrdinalIgnoreCase));

            if (website == null) {
                website = Server.Sites.Add(websiteName, "C:\\inetpub\\wwwroot", 80);
            }

            configureAction(website);

            Server.CommitChanges();
        }

        public void ConfigureWebApplication(string websiteName, string webAppPath, Action<Application> configureAction)
        {
            var website = Server.Sites.FirstOrDefault(x =>
                string.Equals(x.Name, websiteName, StringComparison.OrdinalIgnoreCase));

            if (website == null) throw new ArgumentException($"Website '{websiteName}' was not found!");

            var webApp = website.Applications.FirstOrDefault(x =>
                string.Equals(x.Path, webAppPath, StringComparison.OrdinalIgnoreCase));

            if (webApp == null) {
                webApp = website.Applications.Add(webAppPath, "C:\\inetpub\\wwwroot");
            }

            configureAction(webApp);

            Server.CommitChanges();
        }
    }
}
