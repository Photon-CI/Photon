using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Applications
{
    public interface IApplicationReader
    {
        Task<DomainApplicationRevision> GetRevision(string appName, CancellationToken token = default);
        Task<DomainApplicationRevision> GetRevision(string projectId, string appName, uint deploymentNumber, CancellationToken token = default);
    }
}
