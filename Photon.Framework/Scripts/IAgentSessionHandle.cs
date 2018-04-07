using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public interface IAgentSessionHandle
    {
        Task BeginAsync(string packageId, string packageVersion);
        Task ReleaseAsync();
        Task RunTaskAsync(string taskName);
    }
}
