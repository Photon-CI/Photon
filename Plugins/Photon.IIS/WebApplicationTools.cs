using Microsoft.Web.Administration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                using (var block = handle.Context.Output.WriteBlock()) {
                    block.Write("WebSite ", ConsoleColor.DarkRed);
                    block.Write(webSiteName, ConsoleColor.Red);
                    block.WriteLine(" was not found!", ConsoleColor.DarkRed);
                }

                throw new Exception($"WebSite '{webSiteName}' was not found!");
            }

            if (!TryFind(webSite, webAppPath, out var webApp)) {
                webSite.Applications.Add(webAppPath, DefaultPhysicalPath);
                handle.CommitChanges();

                if (!TryFindWebSite(webSiteName, out webSite)) {
                    using (var block = handle.Context.Output.WriteBlock()) {
                        block.Write("WebSite ", ConsoleColor.DarkRed);
                        block.Write(webSiteName, ConsoleColor.Red);
                        block.WriteLine(" was not found!", ConsoleColor.DarkRed);
                    }

                    throw new Exception($"WebSite '{webSiteName}' was not found!");
                }

                if (!TryFind(webSite, webAppPath, out webApp)) {
                    using (var block = handle.Context.Output.WriteBlock()) {
                        block.Write("Unable to create Web Application ", ConsoleColor.DarkRed);
                        block.Write(webAppPath, ConsoleColor.Red);
                        block.WriteLine("!", ConsoleColor.DarkRed);
                    }

                    throw new Exception($"Unable to create Web Application '{webAppPath}'!");
                }

                using (var block = handle.Context.Output.WriteBlock()) {
                    block.Write("Created new Web Application ", ConsoleColor.DarkBlue);
                    block.Write(webAppPath, ConsoleColor.Blue);
                    block.Write(" under WebSite ", ConsoleColor.DarkBlue);
                    block.Write(webSiteName, ConsoleColor.Blue);
                    block.WriteLine(".", ConsoleColor.DarkBlue);
                }
            }

            configureAction(webApp);

            handle.CommitChanges();

            using (var block = handle.Context.Output.WriteBlock()) {
                block.Write("Web Application ", ConsoleColor.DarkGreen);
                block.Write(webAppPath, ConsoleColor.Green);
                block.WriteLine(" configured successfully.", ConsoleColor.DarkGreen);
            }
        }

        public async Task ConfigureAsync(string webSiteName, string webAppPath, Action<Application> configureAction, CancellationToken token = default(CancellationToken))
        {
            if (!TryFindWebSite(webSiteName, out var webSite)) {
                using (var block = handle.Context.Output.WriteBlock()) {
                    block.Write("WebSite ", ConsoleColor.DarkRed);
                    block.Write(webSiteName, ConsoleColor.Red);
                    block.WriteLine(" was not found!", ConsoleColor.DarkRed);
                }

                throw new Exception($"WebSite '{webSiteName}' was not found!");
            }

            if (!TryFind(webSite, webAppPath, out var webApp)) {
                webSite.Applications.Add(webAppPath, DefaultPhysicalPath);
                await handle.CommitChangesAsync(token);

                if (!TryFindWebSite(webSiteName, out webSite)) {
                    using (var block = handle.Context.Output.WriteBlock()) {
                        block.Write("WebSite ", ConsoleColor.DarkRed);
                        block.Write(webSiteName, ConsoleColor.Red);
                        block.WriteLine(" was not found!", ConsoleColor.DarkRed);
                    }

                    throw new Exception($"WebSite '{webSiteName}' was not found!");
                }

                if (!TryFind(webSite, webAppPath, out webApp)) {
                    using (var block = handle.Context.Output.WriteBlock()) {
                        block.Write("Unable to create Web Application ", ConsoleColor.DarkRed);
                        block.Write(webAppPath, ConsoleColor.Red);
                        block.WriteLine("!", ConsoleColor.DarkRed);
                    }

                    throw new Exception($"Unable to create Web Application '{webAppPath}'!");
                }

                using (var block = handle.Context.Output.WriteBlock()) {
                    block.Write("Created new Web Application ", ConsoleColor.DarkBlue);
                    block.Write(webAppPath, ConsoleColor.Blue);
                    block.Write(" under WebSite ", ConsoleColor.DarkBlue);
                    block.Write(webSiteName, ConsoleColor.Blue);
                    block.WriteLine(".", ConsoleColor.DarkBlue);
                }
            }

            configureAction(webApp);

            await handle.CommitChangesAsync(token);

            using (var block = handle.Context.Output.WriteBlock()) {
                block.Write("Web Application ", ConsoleColor.DarkGreen);
                block.Write(webAppPath, ConsoleColor.Green);
                block.WriteLine(" configured successfully.", ConsoleColor.DarkGreen);
            }
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
