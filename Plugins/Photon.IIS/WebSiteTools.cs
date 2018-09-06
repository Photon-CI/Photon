using Microsoft.Web.Administration;
using Photon.Framework.Extensions;
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

        public async Task<bool> StartAsync(string websiteName, TimeSpan timeout)
        {
            if (!TryFind(websiteName, out var website)) {
                handle.Context.WriteErrorBlock($"IIS WebSite '{websiteName}' not found!");
                return false;
            }

            if (website.State == ObjectState.Started) {
                handle.Context.WriteTagLine($"IIS WebSite '{websiteName}' is already started.", ConsoleColor.White);
                return true;
            }

            website.Start();

            var startTime = DateTime.Now;
            while (website.State != ObjectState.Started && DateTime.Now - startTime < timeout) {
                await Task.Delay(400);
            }

            if (website.State != ObjectState.Started) {
                handle.Context.WriteErrorBlock($"A timeout was reached after {timeout} waiting for IIS WebSite '{websiteName}' to start!");
                return false;
            }

            handle.Context.WriteTagLine($"IIS WebSite '{websiteName}' started successfully.", ConsoleColor.White);
            return true;
        }

        public async Task<bool> StopAsync(string websiteName, TimeSpan timeout)
        {
            if (!TryFind(websiteName, out var website)) {
                handle.Context.WriteErrorBlock($"IIS WebSite '{websiteName}' not found!");
                return false;
            }

            if (website.State == ObjectState.Stopped) {
                handle.Context.WriteTagLine($"IIS WebSite '{websiteName}' is already stopped.", ConsoleColor.White);

                return true;
            }

            website.Stop();

            var startTime = DateTime.Now;
            while (website.State != ObjectState.Stopped && DateTime.Now - startTime < timeout) {
                await Task.Delay(400);
            }

            if (website.State != ObjectState.Stopped) {
                handle.Context.WriteErrorBlock($"A timeout was reached after {timeout} waiting for IIS WebSite '{websiteName}' to stop!");
                return false;
            }

            handle.Context.WriteTagLine($"IIS WebSite '{websiteName}' stopped successfully.", ConsoleColor.White);
            return true;
        }

        public void Configure(string websiteName, int port, Action<Site> configureAction)
        {
            if (!TryFind(websiteName, out var webSite)) {
                handle.Context.WriteTagLine($"Creating IIS WebSite '{websiteName}'...", ConsoleColor.White);

                try {
                    handle.Server.Sites.Add(websiteName, DefaultPhysicalPath, port);
                    handle.CommitChanges();
                }
                catch (Exception error) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS WebSite '{websiteName}'!", error.UnfoldMessages());
                    throw;
                }

                if (!TryFind(websiteName, out webSite)) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS WebSite '{websiteName}'!");
                    throw new Exception($"Failed to create IIS WebSite '{websiteName}'!");
                }

                handle.Context.WriteActionBlock($"Created new IIS WebSite '{websiteName}' successfully.");
            }

            try {
                handle.Context.WriteTagLine($"Configuring IIS WebSite '{websiteName}'...", ConsoleColor.White);

                // TODO: Set Port
                //webSite.Bindings.FirstOrDefault()?.po

                configureAction(webSite);

                handle.CommitChanges();

                handle.Context.WriteTagLine($"Configured IIS WebSite '{websiteName}' successfully.", ConsoleColor.White);
            }
            catch (Exception error) {
                handle.Context.WriteErrorBlock($"Failed to configure IIS WebSite '{websiteName}'!", error.UnfoldMessages());
                throw;
            }
        }

        public async Task ConfigureAsync(string websiteName, int port, Action<Site> configureAction, CancellationToken token = default(CancellationToken))
        {
            if (!TryFind(websiteName, out var webSite)) {
                handle.Context.WriteTagLine($"Creating IIS WebSite '{websiteName}'...", ConsoleColor.White);

                try {
                    handle.Server.Sites.Add(websiteName, DefaultPhysicalPath, port);
                    await handle.CommitChangesAsync(token);
                }
                catch (Exception error) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS WebSite '{websiteName}'!", error.UnfoldMessages());
                    throw;
                }

                if (!TryFind(websiteName, out webSite)) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS WebSite '{websiteName}'!");
                    throw new Exception($"Failed to create IIS WebSite '{websiteName}'!");
                }

                handle.Context.WriteTagLine($"Created IIS WebSite '{websiteName}' successfully.", ConsoleColor.White);
            }

            try {
                handle.Context.WriteTagLine($"Configuring IIS WebSite '{websiteName}'...", ConsoleColor.White);

                // TODO: Set Port
                //webSite.Bindings.FirstOrDefault()?.po

                configureAction(webSite);

                await handle.CommitChangesAsync(token);

                handle.Context.WriteTagLine($"Configured IIS WebSite '{websiteName}' successfully.", ConsoleColor.White);
            }
            catch (Exception error) {
                handle.Context.WriteErrorBlock($"Failed to configure IIS WebSite '{websiteName}'!", error.UnfoldMessages());
                throw;
            }
        }

        private bool TryFind(string webSiteName, out Site webSite)
        {
            webSite = handle.Server.Sites.FirstOrDefault(x => string.Equals(x.Name, webSiteName, StringComparison.OrdinalIgnoreCase));
            return webSite != null;
        }
    }
}
