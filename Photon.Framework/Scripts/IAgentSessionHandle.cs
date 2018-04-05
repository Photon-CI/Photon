using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public interface IAgentSessionHandle
    {
        Task BeginAsync();
        Task ReleaseAsync();
        Task RunTaskAsync(string taskName);
    }
}
