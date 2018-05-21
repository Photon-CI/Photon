using Microsoft.Web.Administration;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                handle.Context.Output
                    .Append("Application Pool ", ConsoleColor.DarkYellow)
                    .Append(appPoolName, ConsoleColor.Yellow)
                    .AppendLine(" not found.", ConsoleColor.DarkYellow);

                return false;
            }

            if (appPool.State == ObjectState.Started) {
                handle.Context.Output
                    .Append("Application Pool ", ConsoleColor.DarkBlue)
                    .Append(appPoolName, ConsoleColor.Blue)
                    .AppendLine(" is already started.", ConsoleColor.DarkBlue);

                return true;
            }

            appPool.Start();

            // TODO: Is this really necessary here?
            //Server.CommitChanges();

            var startTime = DateTime.Now;
            while (appPool.State != ObjectState.Started && DateTime.Now - startTime < timeout) {
                await Task.Delay(400);
            }

            if (appPool.State != ObjectState.Started) {
                handle.Context.Output
                    .Append("A timeout was reached after ", ConsoleColor.DarkRed)
                    .Append(timeout, ConsoleColor.Red)
                    .Append(" waiting for Application Pool ", ConsoleColor.DarkRed)
                    .Append(appPoolName, ConsoleColor.Red)
                    .AppendLine(" to start!", ConsoleColor.DarkRed);

                return false;
            }

            handle.Context.Output
                .Append("Application Pool ", ConsoleColor.DarkGreen)
                .Append(appPoolName, ConsoleColor.Green)
                .AppendLine(" started successfully.", ConsoleColor.DarkGreen);

            return true;
        }

        public async Task<bool> StopAsync(string appPoolName, TimeSpan timeout)
        {
            if (!TryFind(appPoolName, out var appPool)) {
                handle.Context.Output
                    .Append("Application Pool ", ConsoleColor.DarkYellow)
                    .Append(appPoolName, ConsoleColor.Yellow)
                    .AppendLine(" not found.", ConsoleColor.DarkYellow);

                return false;
            }

            if (appPool.State == ObjectState.Stopped) {
                handle.Context.Output
                    .Append("Application Pool ", ConsoleColor.DarkBlue)
                    .Append(appPoolName, ConsoleColor.Blue)
                    .AppendLine(" is already stopped.", ConsoleColor.DarkBlue);

                return true;
            }

            appPool.Stop();

            // TODO: Is this really necessary here?
            //Server.CommitChanges();

            var startTime = DateTime.Now;
            while (appPool.State != ObjectState.Stopped && DateTime.Now - startTime < timeout) {
                await Task.Delay(400);
            }

            if (appPool.State != ObjectState.Stopped) {
                handle.Context.Output
                    .Append("A timeout was reached after ", ConsoleColor.DarkRed)
                    .Append(timeout, ConsoleColor.Red)
                    .Append(" waiting for Application Pool ", ConsoleColor.DarkRed)
                    .Append(appPoolName, ConsoleColor.Red)
                    .AppendLine(" to stop!", ConsoleColor.DarkRed);

                return false;
            }

            handle.Context.Output
                .Append("Application Pool ", ConsoleColor.DarkGreen)
                .Append(appPoolName, ConsoleColor.Green)
                .AppendLine(" stopped successfully.", ConsoleColor.DarkGreen);

            return true;
        }

        public void Configure(string appPoolName, Action<ApplicationPool> configureAction)
        {
            if (!TryFind(appPoolName, out var appPool)) {
                handle.Server.ApplicationPools.Add(appPoolName);
                handle.CommitChanges();

                if (!TryFind(appPoolName, out appPool)) {
                    handle.Context.Output
                        .Append("Unable to create Application Pool ", ConsoleColor.DarkRed)
                        .Append(appPoolName, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed);

                    throw new Exception($"Unable to create Application Pool '{appPoolName}'!");
                }

                handle.Context.Output
                    .Append("Created new Application Pool ", ConsoleColor.DarkBlue)
                    .Append(appPoolName, ConsoleColor.Blue)
                    .AppendLine(".", ConsoleColor.DarkBlue);
            }

            configureAction(appPool);

            handle.CommitChanges();

            handle.Context.Output
                .Append("Application Pool ", ConsoleColor.DarkGreen)
                .Append(appPoolName, ConsoleColor.Green)
                .AppendLine(" configured successfully.", ConsoleColor.DarkGreen);
        }

        private bool TryFind(string appPoolName, out ApplicationPool appPool)
        {
            appPool = handle.Server.ApplicationPools.FirstOrDefault(x => string.Equals(x.Name, appPoolName, StringComparison.OrdinalIgnoreCase));
            return appPool != null;
        }
    }
}
