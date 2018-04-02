using System.Threading.Tasks;

namespace Photon.CLI.Internal.Commands
{
    internal interface ICommandParser
    {
        Task ParseAsync(string[] args);
    }
}
