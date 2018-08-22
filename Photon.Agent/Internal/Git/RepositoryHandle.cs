using log4net;
using Photon.Agent.Internal.Session;
using System;
using System.Threading;

namespace Photon.Agent.Internal.Git
{
    internal class RepositoryHandle : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RepositoryHandle));

        private readonly Action disposeAction;

        public SessionOutput Output {get; set;}
        public RepositorySource Source {get;}
        public string Username {get; set;}
        public string Password {get; set;}
        public bool UseCommandLine {get; set;}
        public bool EnableTracing {get; set;}
        public string CommandLineExe {get; set;}
        public ICheckout Module {get; private set;}

        public RepositoryHandle(RepositorySource source, Action disposeAction)
        {
            this.Source = source;
            this.disposeAction = disposeAction;
        }

        public void Dispose()
        {
            disposeAction?.Invoke();
        }

        public void Checkout(string refspec = "master", CancellationToken token = default(CancellationToken))
        {
            Module = null;

            if (UseCommandLine) {
                Log.Debug("Using Git command-line.");
                Output.WriteLine("Using Git Command-Line.", ConsoleColor.DarkCyan);

                Module = new CmdCheckout {
                    Output = Output.Writer,
                    Source = Source,
                    Username = Username,
                    Password = Password,
                    EnableTracing = EnableTracing,
                };
            }
            else {
                Log.Debug("Using Git Core.");
                Output.WriteLine("Using Git Core.", ConsoleColor.DarkCyan);

                Module = new LibCheckout {
                    Output = Output.Writer,
                    Source = Source,
                    Username = Username,
                    Password = Password,
                    // TODO: Tracing
                };
            }

            Module.Checkout(refspec, token);
        }
    }
}
