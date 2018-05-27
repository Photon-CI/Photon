using Photon.Agent.Internal.Session;
using System;

namespace Photon.Agent.Internal.Git
{
    internal class RepositoryHandle : IDisposable
    {
        private readonly Action disposeAction;

        public SessionOutput Output {get; set;}
        public RepositorySource Source {get;}
        public string Username {get; set;}
        public string Password {get; set;}
        public bool UseCommandLine {get; set;}
        public string CommandLineExe {get; set;}


        public RepositoryHandle(RepositorySource source, Action disposeAction)
        {
            this.Source = source;
            this.disposeAction = disposeAction;
        }

        public void Dispose()
        {
            disposeAction?.Invoke();
        }

        public void Checkout(string refspec = "master")
        {
            ICheckout checkout;
            if (UseCommandLine) {
                checkout = new CmdCheckout {
                    Output = Output,
                    Source = Source,
                    Username = Username,
                    Password = Password,
                };
            }
            else {
                checkout = new LibCheckout {
                    Output = Output,
                    Source = Source,
                    Username = Username,
                    Password = Password,
                };
            }

            checkout.Checkout(refspec);
        }
    }
}
