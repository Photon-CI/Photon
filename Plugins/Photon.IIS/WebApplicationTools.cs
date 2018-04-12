using Microsoft.Web.Administration;
using Photon.Framework.Domain;
using System;
using System.Linq;

namespace Photon.Plugins.IIS
{
    public class WebApplicationTools
    {
        private const string DefaultPhysicalPath = "C:\\inetpub\\wwwroot";

        public IDomainContext Context {get;}
        public ServerManager Server {get;}


        internal WebApplicationTools(IDomainContext context, ServerManager server)
        {
            this.Context = context;
            this.Server = server;
        }

        public void Configure(string webSiteName, string webAppPath, Action<Application> configureAction)
        {
            if (!TryFindWebSite(webSiteName, out var webSite)) {
                Context.Output
                    .Append("WebSite ", ConsoleColor.DarkRed)
                    .Append(webSiteName, ConsoleColor.Red)
                    .AppendLine(" was not found!", ConsoleColor.DarkRed);

                throw new Exception($"WebSite '{webSiteName}' was not found!");
            }

            if (!TryFind(webSite, webAppPath, out var webApp)) {
                webSite.Applications.Add(webAppPath, DefaultPhysicalPath);
                Server.CommitChanges();

                if (!TryFind(webSite, webAppPath, out webApp)) {
                    Context.Output
                        .Append("Unable to create Web Application ", ConsoleColor.DarkRed)
                        .Append(webSiteName, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed);

                    throw new Exception($"Unable to create Web Application '{webSiteName}'!");
                }

                Context.Output
                    .Append("Created new Web Application ", ConsoleColor.DarkBlue)
                    .Append(webAppPath, ConsoleColor.Blue)
                    .Append(" under WebSite ", ConsoleColor.DarkBlue)
                    .Append(webSiteName, ConsoleColor.Blue)
                    .AppendLine(".", ConsoleColor.DarkBlue);
            }

            configureAction(webApp);

            Server.CommitChanges();

            Context.Output
                .Append("Web Application ", ConsoleColor.DarkGreen)
                .Append(webSiteName, ConsoleColor.Green)
                .AppendLine(" configured successfully.", ConsoleColor.DarkGreen);
        }

        private bool TryFindWebSite(string webSiteName, out Site webSite)
        {
            webSite = Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }

        private bool TryFind(Site webSite, string webAppPath, out Application webApp)
        {
            webApp = webSite.Applications.FirstOrDefault(x => string.Equals(x.Path, webAppPath, StringComparison.OrdinalIgnoreCase));
            return webApp != null;
        }
    }
}
