using System.Threading;

namespace Photon.Agent.Internal.Git
{
    internal interface ICheckout
    {
        void Checkout(string refspec = "master", CancellationToken token = default(CancellationToken));
    }
}
