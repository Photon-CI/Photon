using Microsoft.Web.Administration;
using System;
using System.Linq;

namespace Photon.Plugins.IIS
{
    public class WebSiteTools
    {
        private const string DefaultPhysicalPath = "%SystemRoot%\\inetpub\\wwwroot";

        private readonly IISServerHandle handle;


        internal WebSiteTools(IISServerHandle handle)
        {
            this.handle = handle;
        }

        public void Configure(string websiteName, int port, Action<Site> configureAction)
        {
            if (!TryFind(websiteName, out var webSite)) {
                handle.Server.Sites.Add(websiteName, DefaultPhysicalPath, port);
                handle.CommitChanges();

                if (!TryFind(websiteName, out webSite)) {
                    handle.Context.Output.WriteBlock()
                        .Write("Unable to create WebSite ", ConsoleColor.DarkRed)
                        .Write(websiteName, ConsoleColor.Red)
                        .WriteLine("!", ConsoleColor.DarkRed)
                        .Post();

                    throw new Exception($"Unable to create WebSite '{websiteName}'!");
                }

                handle.Context.Output.WriteBlock()
                    .Write("Created new WebSite ", ConsoleColor.DarkBlue)
                    .Write(websiteName, ConsoleColor.Blue)
                    .WriteLine(".", ConsoleColor.DarkBlue)
                    .Post();
            }

            // TODO: Set Port
            //webSite.Bindings.FirstOrDefault()?.po

            configureAction(webSite);

            handle.CommitChanges();

            handle.Context.Output.WriteBlock()
                .Write("WebSite ", ConsoleColor.DarkGreen)
                .Write(websiteName, ConsoleColor.Green)
                .WriteLine(" configured successfully.", ConsoleColor.DarkGreen)
                .Post();
        }

        private bool TryFind(string webSiteName, out Site webSite)
        {
            webSite = handle.Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }
    }
}
