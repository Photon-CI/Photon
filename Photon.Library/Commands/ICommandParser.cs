using System.Threading.Tasks;

namespace Photon.Library.Commands
{
    public interface ICommandParser
    {
        Task ParseAsync(string[] args);
    }
}
