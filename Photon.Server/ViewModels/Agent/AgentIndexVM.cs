using System.Linq;
using Photon.Framework.Server;
using Photon.Library;
using Photon.Server.Internal;

namespace Photon.Server.ViewModels.Agent
{
    internal class AgentIndexVM : ViewModelBase
    {
        public ServerAgent[] Agents {get; set;}


        public void Build()
        {
            Agents = PhotonServer.Instance.Agents.All.ToArray();
        }
    }
}
