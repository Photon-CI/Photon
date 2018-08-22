using System.Threading;

namespace Photon.Agent.Internal.Git
{
    internal interface ICheckout
    {
        string CommitHash {get;}
        string CommitAuthor {get;}
        string CommitMessage {get;}

        void Checkout(string refspec = "master", CancellationToken token = default(CancellationToken));
    }
}
