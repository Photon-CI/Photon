using Photon.Framework.Domain;
using System.Threading.Tasks;

namespace Photon.Framework.Agent
{
    public interface IAgentContext : IDomainContext
    {
        void RunCommandLine(string command);
        void RunCommandLine(string command, params string[] args);
        Task PushProjectPackageAsync(string filename);
        Task PushApplicationPackageAsync(string filename);
        Task PullProjectPackageAsync(string id, string version, string filename);
        Task PullApplicationPackageAsync(string id, string version, string filename);
    }
}
