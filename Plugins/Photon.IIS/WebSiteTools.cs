using Microsoft.Web.Administration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    using (var block = handle.Context.Output.WriteBlock()) {
                        block.Write("Unable to create WebSite ", ConsoleColor.DarkRed);
                        block.Write(websiteName, ConsoleColor.Red);
                        block.WriteLine("!", ConsoleColor.DarkRed);
                    }

                    throw new Exception($"Unable to create WebSite '{websiteName}'!");
                }

                using (var block = handle.Context.Output.WriteBlock()) {
                    block.Write("Created new WebSite ", ConsoleColor.DarkBlue);
                    block.Write(websiteName, ConsoleColor.Blue);
                    block.WriteLine(".", ConsoleColor.DarkBlue);
                }
            }

            // TODO: Set Port
            //webSite.Bindings.FirstOrDefault()?.po

            configureAction(webSite);

            handle.CommitChanges();

            using (var block = handle.Context.Output.WriteBlock()) {
                block.Write("WebSite ", ConsoleColor.DarkGreen);
                block.Write(websiteName, ConsoleColor.Green);
                block.WriteLine(" configured successfully.", ConsoleColor.DarkGreen);
            }
        }

        public async Task ConfigureAsync(string websiteName, int port, Action<Site> configureAction, CancellationToken token = default(CancellationToken))
        {
            if (!TryFind(websiteName, out var webSite)) {
                handle.Server.Sites.Add(websiteName, DefaultPhysicalPath, port);
                await handle.CommitChangesAsync(token);

                if (!TryFind(websiteName, out webSite)) {
                    using (var block = handle.Context.Output.WriteBlock()) {
                        block.Write("Unable to create WebSite ", ConsoleColor.DarkRed);
                        block.Write(websiteName, ConsoleColor.Red);
                        block.WriteLine("!", ConsoleColor.DarkRed);
                    }

                    throw new Exception($"Unable to create WebSite '{websiteName}'!");
                }

                using (var block = handle.Context.Output.WriteBlock()) {
                    block.Write("Created new WebSite ", ConsoleColor.DarkBlue);
                    block.Write(websiteName, ConsoleColor.Blue);
                    block.WriteLine(".", ConsoleColor.DarkBlue);
                }
            }

            // TODO: Set Port
            //webSite.Bindings.FirstOrDefault()?.po

            configureAction(webSite);

            await handle.CommitChangesAsync(token);

            using (var block = handle.Context.Output.WriteBlock()) {
                block.Write("WebSite ", ConsoleColor.DarkGreen);
                block.Write(websiteName, ConsoleColor.Green);
                block.WriteLine(" configured successfully.", ConsoleColor.DarkGreen);
            }
        }

        private bool TryFind(string webSiteName, out Site webSite)
        {
            webSite = handle.Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }
    }
}
