using Photon.Framework.Server;
using Photon.Server.Internal;
using System.Linq;

namespace Photon.Server.ViewModels.Agent
{
    internal class AgentIndexVM : ServerViewModel
    {
        public ServerAgent[] Agents {get; set;}


        public void Build()
        {
            Agents = PhotonServer.Instance.Agents.All
                .OrderBy(x => x.Name).ToArray();
        }
    }
}
