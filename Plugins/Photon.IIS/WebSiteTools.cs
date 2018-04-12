using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;
using System.Linq;

namespace Photon.Plugins.IIS
{
    public class WebSiteTools
    {
        private const string DefaultPhysicalPath = "C:\\inetpub\\wwwroot";

        public IDomainContext Context {get;}
        public ServerManager Server {get;}


        internal WebSiteTools(IDomainContext context, ServerManager server)
        {
            this.Context = context;
            this.Server = server;
        }

        public void Configure(string websiteName, int port, Action<Site> configureAction)
        {
            if (!TryFind(websiteName, out var webSite)) {
                Server.Sites.Add(websiteName, DefaultPhysicalPath, port);
                Server.CommitChanges();

                if (!TryFind(websiteName, out webSite)) {
                    Context.Output
                        .Append("Unable to create WebSite ", ConsoleColor.DarkRed)
                        .Append(websiteName, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed);

                    throw new Exception($"Unable to create WebSite '{websiteName}'!");
                }

                Context.Output
                    .Append("Created new WebSite ", ConsoleColor.DarkBlue)
                    .Append(websiteName, ConsoleColor.Blue)
                    .AppendLine(".", ConsoleColor.DarkBlue);
            }

            // TODO: Set Port
            //webSite.Bindings.FirstOrDefault()?.po

            configureAction(webSite);

            Server.CommitChanges();

            Context.Output
                .Append("WebSite ", ConsoleColor.DarkGreen)
                .Append(websiteName, ConsoleColor.Green)
                .AppendLine(" configured successfully.", ConsoleColor.DarkGreen);
        }

        private bool TryFind(string webSiteName, out Site webSite)
        {
            webSite = Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }
    }
}
