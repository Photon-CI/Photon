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
                    handle.Context.Output
                        .Append("Unable to create WebSite ", ConsoleColor.DarkRed)
                        .Append(websiteName, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed);

                    throw new Exception($"Unable to create WebSite '{websiteName}'!");
                }

                handle.Context.Output
                    .Append("Created new WebSite ", ConsoleColor.DarkBlue)
                    .Append(websiteName, ConsoleColor.Blue)
                    .AppendLine(".", ConsoleColor.DarkBlue);
            }

            // TODO: Set Port
            //webSite.Bindings.FirstOrDefault()?.po

            configureAction(webSite);

            handle.CommitChanges();

            handle.Context.Output
                .Append("WebSite ", ConsoleColor.DarkGreen)
                .Append(websiteName, ConsoleColor.Green)
                .AppendLine(" configured successfully.", ConsoleColor.DarkGreen);
        }

        private bool TryFind(string webSiteName, out Site webSite)
        {
            webSite = handle.Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }
    }
}
