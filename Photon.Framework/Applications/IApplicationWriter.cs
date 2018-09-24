using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Applications
{
    public interface IApplicationWriter : IApplicationReader
    {
        Task<DomainApplicationRevision> RegisterRevision(string appName, string packageId, string packageVersion, string environmentName = null, CancellationToken token = default);
        Task<DomainApplicationRevision> RegisterRevision(DomainApplicationRevisionRequest revisionRequest, CancellationToken token = default);
    }
}
