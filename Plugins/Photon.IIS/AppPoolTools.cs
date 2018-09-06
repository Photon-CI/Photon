using Microsoft.Web.Administration;
using Photon.Framework.Extensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace Photon.Plugins.IIS
{
    public class ApplicationPoolTools
    {
        private readonly IISServerHandle handle;


        internal ApplicationPoolTools(IISServerHandle handle)
        {
            this.handle = handle;
        }

        public async Task<bool> StartAsync(string appPoolName, TimeSpan timeout)
        {
            if (!TryFind(appPoolName, out var appPool)) {
                handle.Context.WriteErrorBlock($"IIS Application Pool '{appPoolName}' not found!");
                return false;
            }

            if (appPool.State == ObjectState.Started) {
                handle.Context.WriteTagLine($"IIS Application Pool '{appPoolName}' is already started.", ConsoleColor.White);
                return true;
            }

            appPool.Start();

            var startTime = DateTime.Now;
            while (appPool.State != ObjectState.Started && DateTime.Now - startTime < timeout) {
                await Task.Delay(400);
            }

            if (appPool.State != ObjectState.Started) {
                handle.Context.WriteErrorBlock($"A timeout was reached after {timeout} waiting for IIS Application Pool '{appPoolName}' to start!");
                return false;
            }

            handle.Context.WriteTagLine($"IIS Application Pool '{appPoolName}' started successfully.", ConsoleColor.White);
            return true;
        }

        public async Task<bool> StopAsync(string appPoolName, TimeSpan timeout)
        {
            if (!TryFind(appPoolName, out var appPool)) {
                handle.Context.WriteErrorBlock($"IIS Application Pool '{appPoolName}' not found!");
                return false;
            }

            if (appPool.State == ObjectState.Stopped) {
                handle.Context.WriteTagLine($"IIS Application Pool '{appPoolName}' is already stopped.", ConsoleColor.White);

                return true;
            }

            appPool.Stop();

            var startTime = DateTime.Now;
            while (appPool.State != ObjectState.Stopped && DateTime.Now - startTime < timeout) {
                await Task.Delay(400);
            }

            if (appPool.State != ObjectState.Stopped) {
                handle.Context.WriteErrorBlock($"A timeout was reached after {timeout} waiting for IIS Application Pool '{appPoolName}' to stop!");
                return false;
            }

            handle.Context.WriteTagLine($"IIS Application Pool '{appPoolName}' stopped successfully.", ConsoleColor.White);
            return true;
        }

        public void Configure(string appPoolName, Action<ApplicationPool> configureAction)
        {
            if (!TryFind(appPoolName, out var appPool)) {
                handle.Context.WriteTagLine($"Creating IIS Application Pool '{appPoolName}'...", ConsoleColor.White);

                try {
                    handle.Server.ApplicationPools.Add(appPoolName);
                    handle.CommitChanges();
                }
                catch (Exception error) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS Application Pool '{appPoolName}'!", error.UnfoldMessages());
                    throw;
                }

                if (!TryFind(appPoolName, out appPool)) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS Application Pool '{appPoolName}'!");
                    throw new Exception($"Unable to create IIS Application Pool '{appPoolName}'!");
                }

                handle.Context.WriteTagLine($"Created IIS Application Pool '{appPoolName}' successfully.", ConsoleColor.White);
            }

            try {
                handle.Context.WriteTagLine($"Configuring IIS Application Pool '{appPoolName}'...", ConsoleColor.White);

                configureAction(appPool);

                handle.CommitChanges();

                handle.Context.WriteTagLine($"Configured IIS Application Pool '{appPoolName}'.", ConsoleColor.White);
            }
            catch (Exception error) {
                handle.Context.WriteErrorBlock($"Failed to configure IIS Application Pool '{appPoolName}'!", error.UnfoldMessages());
                throw;
            }
        }

        public async Task ConfigureAsync(string appPoolName, Action<ApplicationPool> configureAction, CancellationToken token = default(CancellationToken))
        {
            if (!TryFind(appPoolName, out var appPool)) {
                handle.Context.WriteTagLine($"Creating IIS Application Pool '{appPoolName}'...", ConsoleColor.White);

                try {
                    handle.Server.ApplicationPools.Add(appPoolName);
                    await handle.CommitChangesAsync(token);
                }
                catch (Exception error) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS Application Pool '{appPoolName}'!", error.UnfoldMessages());
                    throw;
                }

                if (!TryFind(appPoolName, out appPool)) {
                    handle.Context.WriteErrorBlock($"Failed to create IIS Application Pool '{appPoolName}'!");
                    throw new Exception($"Unable to create IIS Application Pool '{appPoolName}'!");
                }

                handle.Context.WriteTagLine($"Created IIS Application Pool '{appPoolName}' successfully.", ConsoleColor.White);
            }

            try {
                handle.Context.WriteTagLine($"Configuring IIS Application Pool '{appPoolName}'...", ConsoleColor.White);

                configureAction(appPool);

                await handle.CommitChangesAsync(token);

                handle.Context.WriteTagLine($"Configured IIS Application Pool '{appPoolName}' successfully.", ConsoleColor.White);
            }
            catch (Exception error) {
                handle.Context.WriteErrorBlock($"Failed to configure IIS Application Pool '{appPoolName}'!", error.UnfoldMessages());
                throw;
            }
        }

        private bool TryFind(string appPoolName, out ApplicationPool appPool)
        {
            appPool = handle.Server.ApplicationPools.FirstOrDefault(x => string.Equals(x.Name, appPoolName, StringComparison.OrdinalIgnoreCase));
            return appPool != null;
        }
    }
}
