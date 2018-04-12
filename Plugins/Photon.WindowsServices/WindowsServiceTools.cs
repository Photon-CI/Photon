using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using TimeoutException = System.TimeoutException;

namespace Photon.WindowsServices
{
    public class WindowsServiceTools
    {
        public IDomainContext Context {get;}


        public WindowsServiceTools(IDomainContext context)
        {
            this.Context = context;
        }

        public bool Start(string serviceName)
        {
            ServiceController service = null;
            var timeout = TimeSpan.FromMinutes(1);

            try {
                service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Running) {
                    Context.Output
                        .Append("Service ", ConsoleColor.DarkBlue)
                        .Append(serviceName, ConsoleColor.Blue)
                        .AppendLine(" is already running.", ConsoleColor.DarkBlue);

                    return true;
                }

                Context.Output
                    .Append("Starting Service ", ConsoleColor.DarkCyan)
                    .Append(serviceName, ConsoleColor.Cyan)
                    .AppendLine("...", ConsoleColor.DarkCyan);

                service.Start();

                service.WaitForStatus(ServiceControllerStatus.Running, timeout);

                Context.Output
                    .Append("Service ", ConsoleColor.DarkGreen)
                    .Append(serviceName, ConsoleColor.Green)
                    .AppendLine(" started successfully.", ConsoleColor.DarkGreen);

                return true;
            }
            catch (InvalidOperationException) {
                PrintServiceNotFound(serviceName);
                throw new Exception($"Service '{serviceName}' not found.");
            }
            catch (TimeoutException) {
                PrintTimeout(serviceName, timeout, "start");
                throw;
            }
            catch (Exception error) {
                Context.Output
                    .Append("Failed to start Service ", ConsoleColor.DarkRed)
                    .Append(serviceName, ConsoleColor.Red)
                    .Append("! ", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
            finally {
                service?.Dispose();
            }
        }

        public async Task<bool> StartAsync(string serviceName)
        {
            return await Task.Run(() => Start(serviceName));
        }

        public bool Stop(string serviceName)
        {
            ServiceController service = null;
            var timeout = TimeSpan.FromMinutes(1);

            try {
                service = new ServiceController(serviceName);

                if (service.Status == ServiceControllerStatus.Stopped) {
                    Context.Output
                        .Append("Service ", ConsoleColor.DarkBlue)
                        .Append(serviceName, ConsoleColor.Blue)
                        .AppendLine(" is already stopped.", ConsoleColor.DarkBlue);

                    return true;
                }

                Context.Output
                    .Append("Stopping Service ", ConsoleColor.DarkCyan)
                    .Append(serviceName, ConsoleColor.Cyan)
                    .AppendLine("...", ConsoleColor.DarkCyan);

                service.Stop();

                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                Context.Output
                    .Append("Service ", ConsoleColor.DarkGreen)
                    .Append(serviceName, ConsoleColor.Green)
                    .AppendLine(" stopped successfully.", ConsoleColor.DarkGreen);

                return true;
            }
            catch (InvalidOperationException) {
                PrintServiceNotFound(serviceName);
                return false;
            }
            catch (TimeoutException) {
                PrintTimeout(serviceName, timeout, "stop");
                throw;
            }
            catch (Exception error) {
                Context.Output
                    .Append("Failed to stop Service ", ConsoleColor.DarkRed)
                    .Append(serviceName, ConsoleColor.Red)
                    .Append("! ", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
            finally {
                service?.Dispose();
            }
        }

        public async Task<bool> StopAsync(string serviceName)
        {
            return await Task.Run(() => Stop(serviceName));
        }

        public void Install(string filename, string serviceName)
        {
            throw new NotImplementedException();
        }

        public void Uninstall(string serviceName)
        {
            throw new NotImplementedException();
        }

        private void PrintServiceNotFound(string serviceName)
        {
            Context.Output
                .Append("Service ", ConsoleColor.DarkYellow)
                .Append(serviceName, ConsoleColor.Yellow)
                .AppendLine(" not found.", ConsoleColor.DarkYellow);
        }

        private void PrintTimeout(string serviceName, TimeSpan timeout, string operation)
        {
            Context.Output
                .Append("A timeout occurred after ", ConsoleColor.DarkYellow)
                .Append(timeout, ConsoleColor.Yellow)
                .Append(" waiting for Service ", ConsoleColor.DarkYellow)
                .Append(serviceName, ConsoleColor.Yellow)
                .AppendLine($" to {operation}!", ConsoleColor.DarkYellow);
        }
    }
}
