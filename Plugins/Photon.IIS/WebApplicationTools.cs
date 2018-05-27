using Microsoft.Web.Administration;
using System;
using System.Linq;

namespace Photon.Plugins.IIS
{
    public class WebApplicationTools
    {
        private const string DefaultPhysicalPath = "%SystemRoot%\\inetpub\\wwwroot";

        private readonly IISServerHandle handle;


        internal WebApplicationTools(IISServerHandle handle)
        {
            this.handle = handle;
        }

        public void Configure(string webSiteName, string webAppPath, Action<Application> configureAction)
        {
            if (!TryFindWebSite(webSiteName, out var webSite)) {
                handle.Context.Output
                    .Write("WebSite ", ConsoleColor.DarkRed)
                    .Write(webSiteName, ConsoleColor.Red)
                    .WriteLine(" was not found!", ConsoleColor.DarkRed);

                throw new Exception($"WebSite '{webSiteName}' was not found!");
            }

            if (!TryFind(webSite, webAppPath, out var webApp)) {
                webSite.Applications.Add(webAppPath, DefaultPhysicalPath);
                handle.CommitChanges();

                if (!TryFind(webSite, webAppPath, out webApp)) {
                    handle.Context.Output
                        .Write("Unable to create Web Application ", ConsoleColor.DarkRed)
                        .Write(webSiteName, ConsoleColor.Red)
                        .WriteLine("!", ConsoleColor.DarkRed);

                    throw new Exception($"Unable to create Web Application '{webSiteName}'!");
                }

                handle.Context.Output
                    .Write("Created new Web Application ", ConsoleColor.DarkBlue)
                    .Write(webAppPath, ConsoleColor.Blue)
                    .Write(" under WebSite ", ConsoleColor.DarkBlue)
                    .Write(webSiteName, ConsoleColor.Blue)
                    .WriteLine(".", ConsoleColor.DarkBlue);
            }

            configureAction(webApp);

            handle.CommitChanges();

            handle.Context.Output
                .Write("Web Application ", ConsoleColor.DarkGreen)
                .Write(webSiteName, ConsoleColor.Green)
                .WriteLine(" configured successfully.", ConsoleColor.DarkGreen);
        }

        private bool TryFindWebSite(string webSiteName, out Site webSite)
        {
            webSite = handle.Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }

        private bool TryFind(Site webSite, string webAppPath, out Application webApp)
        {
            webApp = webSite.Applications.FirstOrDefault(x => string.Equals(x.Path, webAppPath, StringComparison.OrdinalIgnoreCase));
            return webApp != null;
        }
    }
}
