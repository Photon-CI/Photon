using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    public interface IPackageClient
    {
        Task PushProjectPackageAsync(string filename, CancellationToken token = default);
        Task<string> PullProjectPackageAsync(string id, string version, CancellationToken token = default);
        Task PushApplicationPackageAsync(string filename, CancellationToken token = default);
        Task<string> PullApplicationPackageAsync(string id, string version, CancellationToken token = default);
    }
}
